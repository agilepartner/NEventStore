$script:XUnitVersionPath = ''
$script:XUnitDefault = 'xunit.console.clr4.exe'
$script:XunitRunnerSpec = 'xunit\.console(\.clr4)?(\.x86)?\.exe$'

function Get-CurrentDirectory
{
    $thisName = $MyInvocation.MyCommand.Name
    [IO.Path]::GetDirectoryName((Get-Content function:$thisName).File)
}

function Set-XUnitPath
{
  <#
  .Synopsis
    Sets a global XUnitPath for use during this session, so that one
    does not have to be provided every time to Invoke-XUnit.
  .Description
    By default, the sibling directories will be scanned once the first
    time Invoke-XUnit is run, to find the xunit.console.clr4.exe - but
    in the event that the assemblies are not hosted in a sibling package
    directory, this provides a mechanism for overloading.
  .Parameter Path
    The full path to xunit.console.clr4.exe, xunit.console.exe,
    xunit.console.clr4.x86.exe or xunit.console.x86.exe
  .Example
    Set-XUnitPath c:\foo\packages\XUnit\lib\tools\xunit.console.clr4.exe
  #>
  [CmdletBinding()]
  param(
    [Parameter(Mandatory=$true)]
    [IO.FileInfo]
    [ValidateScript(
    {
      (Test-Path $_) -and (!$_.PSIsContainer) -and
      ($_.Name -imatch $script:XunitRunnerSpec)
    })]
    $Path
  )

  $script:XUnitVersionPath = $Path
}

function Get-XUnitPath
{
  if ($script:XUnitVersionPath -ne '')
  {
    return $script:XUnitVersionPath
  }

  #assume that the package has been restored in a sibling directory
  $parentDirectory = Split-Path (Split-Path (Get-CurrentDirectory))

  $script:XUnitVersionPath =
    Get-ChildItem -Path $parentDirectory -Filter $script:XUnitDefault -Recurse |
    ? { $_.DirectoryName.EndsWith('tools') } | Select -First 1 -ExpandProperty FullName

  return $script:XUnitVersionPath
}

function New-XUnitProjectFile
{
  <#
  .Synopsis
    Creates a new XUnit project file in the %temp% directory of the
    current user, based on the given file specifications.

    Returns the filename of the project file.
  .Description
    The list of given files is automatically parsed, so that files of the
    same name and length are only included once (should they appear in
    multiple directories).

    By default, convention is to look for assemblies that match the
    *Tests.dll pattern.

    By default, the output results generated by XUnit will be in NUnit
    format, since that is currently the most friendly for build servers
    like Jenkins.

    The files in the temp directory are not automatically cleaned up
    after they are created.
  .Parameter Path
    A list of root paths to search through
  .Parameter TestSpec
    A wildcard specification of files to look for in each of the given
    paths.  This should follow the syntax that the -Include switch of
    Get-ChildItem accepts.

    Defaults to *Tests.dll
  .Example
    New-XUnitProjectFile -Path 'c:\source', 'd:\source'

    Will find all files matching the default *Tests.dll in c:\source and
    d:\source, and will create a project file similar to
    %temp%\tmp1234.tmp.xunit
  .Example
    New-XUnitProjectFile -Path 'd:\' -OutputFormat xunit

    Will find all files matching the default *Tests.dll in d:\ and will
    create a project file similar to %temp%\tmp1234.tmp.xunit.  The test
    results are written in XUnit format
  #>
  [CmdletBinding()]
  param(
    [Parameter(Mandatory=$true)]
    [string[]]
    $Path,

    [Parameter(Mandatory=$false)]
    [string[]]
    $TestSpec = @('*Tests.dll'),

    [Parameter(Mandatory=$false)]
    [string]
    [ValidateSet('xml','html','nunit')]
    $OutputFormat = 'nunit'
  )

  $testAsms = @()
  $uniqueFiles = @{}
  $Path | Get-ChildItem -Recurse -Include $TestSpec |
    ? { (-not $uniqueFiles.Contains($_.Name)) -and `
        ($uniqueFiles[$_.Name] -ne $_.Length) } |
    % { $testAsms += $_; $uniqueFiles[$_.Name] = $_.Length }

  if ($testAsms.Count -eq 0)
    { throw "No test assemblies matching $TestSpec in $Path!" +
    "Do you have no tests?? Run a build first!" }

  $projectPath = ([IO.Path]::GetTempFileName() + '.xunit')
  Copy-Item -Path (Join-Path (Get-CurrentDirectory) 'template.xunit') `
    -Destination $projectPath

  $xml = New-Object XML
  $xml.Load($projectPath)
  $nodeTemplate = @($xml.xunit.assemblies.assembly)[0]

  $testAsms |
    % {
      $test = $nodeTemplate.Clone()
      $test.filename = $_.FullName
      $fileName = "$OutputFormat.TestResult.$($_.Name).xml"
      $test.output.filename = [string](Join-Path $Path $fileName)
      $test.output.type = $OutputFormat
      [Void]$xml.xunit.assemblies.AppendChild($test)
    }

  [Void]$xml.xunit.assemblies.RemoveChild($nodeTemplate)
  $xml.Save($projectPath)
  Write-Host "Wrote XUnit project for $($testAsms.Count) asms to $projectPath"

  return $projectPath
}

filter Select-TestAssemblies
{
  #must be an XmlDocument
  $_.'test-results'.'test-suite'
    #? { ($_.Name -eq 'test-suite') -and ($_.type -eq 'Assembly') }
    #Xunit doesn't set 'type' attribute in 'test-suite' to 'Assembly'
}

function Get-TestSummary
{
  [CmdletBinding()]
  param(
    [Parameter(Mandatory=$true)]
    [Xml.XmlDocument]
    $xDoc
  )

  $tr = $xDoc.'test-results'

  return @{
    total = [Convert]::ToInt32($tr.total);
    errors = [Convert]::ToInt32($tr.errors);
    failures = [Convert]::ToInt32($tr.failures);
    notrun = [Convert]::ToInt32($tr.'not-run');
    inconclusive = [Convert]::ToInt32($tr.inconclusive);
    ignored = [Convert]::ToInt32($tr.ignored);
    skipped = [Convert]::ToInt32($tr.skipped);
    invalid = [Convert]::ToInt32($tr.invalid);
    datetime = [DateTime]::Parse((@($tr.date, $tr.time) -join " "));
  }
}

function Fold-Docs
{
  [CmdletBinding()]
  param(
    [Parameter(Mandatory=$true, ValueFromPipeline=$true)]
    $Docs
  )

  begin
  {
    $Environment = $null
    $Culture = $null
    $Assemblies = @()
  }
  process
  {
    if ($_ -eq $null) { $_ = $Docs }

    if ($Culture -eq $null) { $Culture = $_.'test-results'.'culture-info' }
    if ($Environment -eq $null) { $Environment = $_.'test-results'.environment }

    #this sanity check was in original F# code, but not sure it's a good generic
    #if ((-not (Compare-Hash $Environment (Get-Environment $_))) -or `
    #  (-not (Compare-Hash $Culture (Get-Culture $_))))
    #{ throw "Unmatched environment and/or cultures detected: some of theses results files are not from the same test run." }

    $thisSummary = Get-TestSummary $_
    if ($Summary -eq $null) { $Summary = $thisSummary }
    else
    {
      $Summary.Total += $thisSummary.Total
      $Summary.Errors += $thisSummary.Errors
      $Summary.Failures += $thisSummary.Failures
      $Summary.Notrun += $thisSummary.Notrun
      $Summary.Inconclusive += $thisSummary.Inconclusive
      $Summary.Ignored += $thisSummary.Ignored
      $Summary.Skipped += $thisSummary.Skipped
      $Summary.Invalid += $thisSummary.Invalid
      $Summary.DateTime = if ($Summary.DateTime -lt $thisSummary.DateTime)
        { $Summary.DateTime } else { $thisSummary.DateTime}
    }

    $Assemblies += ($_ | Select-TestAssemblies)
  }
  end
  {
    #leading , is necessary to prevent automatic unrolling
    return ,@($Summary, $Environment, $Culture, $Assemblies)
  }
}

function Merge-Docs
{
  [CmdletBinding()]
  param(
    [Parameter(Mandatory=$true, ValueFromPipeline=$true)]
    $pipelineValues
  )

  process
  {
    if ($_ -eq $null) { $_ = $pipelineValues }
    $summary, $environment, $culture, $assemblies = $_

    #calc some summary data
    $result, $time, $asserts = 'Success', 0.0, 0
    $Assemblies |
    % {
      if (($_.result -eq 'Failure') -or ($result -eq 'Failure'))
        { $result = 'Failure' }
      elseif (($_.result -eq 'Inconclusive') -and ($result -eq 'Success'))
        { $result = 'Inconclusive' }
      $time += [Convert]::ToDouble($_.time)
      $asserts += [Convert]::ToInt32($_.asserts)
    }

@"
<test-results name='Merged results' total='$($summary.total)'
errors='$($summary.errors)' failures='$($summary.failures)'
not-run='$($summary.notrun)' inconclusive='$($summary.inconclusive)'
skipped='$($summary.skipped)' invalid='$($summary.invalid)'
date='$($summary.datetime.ToString('yyyy-MM-dd'))'
time='$($summary.datetime.ToString('HH:mm:ss'))'>

  $($environment.OuterXml)
  $($culture.OuterXml)

  <test-suite type='Test Project' name='' executed='True' result='$result'
  time='$time' asserts='$asserts'>
    <results>
    $($Assemblies | % { $_.OuterXml })
    </results>
  </test-suite>
</test-results>
"@
  }
}

function New-MergedNUnitXml
{
  <#
  .Synopsis
    Takes NUnit xml results files, and creates a merged version of the
    results at the given path, summarizing the run time, result counts
    and success status.
  .Description
    The resulting file has a top level 'test-results' node containing
    the merged / summarized information, followed by the first available
    environment and culture information should it exist.

    The results from the various files are then dumped under a
    'test-suite' node that contains a Failure / Inconclusive / Success
    status based on a cumulative summary of the other nodes.  It also
    contains the total run time.

    Note that the XUnit XSLT doesn't currently produce NUnit with all the
    details filled in.  Environment and Culture information are missing.
  .Parameter Path
    A list of paths to include, may include wildcards

    Accepts the same values as the -Path switch of Get-Item
  .Parameter Destination
    An output file for the results.  If the file exists already, it will
    be deleted.
  .Example
    New-MergedNUnitXml -Path 'c:\nunit*.xml' -Destination nunit.xml

    Will find all files matching nunit*.xml specification in c:\,
    merging and summarizing them into nunit.xml
  #>
  [CmdletBinding()]
  param(
    [Parameter(Mandatory=$true)]
    [string[]]
    $Path,

    [Parameter(Mandatory=$true)]
    [string]
    $Destination
  )

  $files = Get-Item -Path $Path -ErrorAction SilentlyContinue |
    % { [Xml](Get-Content $_) }

  if (($files -eq $null) -or ($files.Count -eq 0))
    { throw "No input files could be found for merging" }

  Write-Host "Merging $($files.Count) nunit results into file: $($Destination)"

  #influence - https://github.com/15below/NUnitMerger/blob/master/Core.fs
  #shortened quite a bit, since powershell has fancy xml dot notation
  $xml = $files |
    Fold-Docs |
    Merge-Docs

  $xml = [Xml]('<?xml version="1.0" encoding="utf-8" ?>' + $xml)
  if ($xml.'test-results'.'test-suite'.result -ne 'Success')
  {
    Write-Host "Some tests failed."
  }
  $xml.Save($Destination)
}

function Invoke-XUnit
{
  <#
  .Synopsis
    Runs XUnit over a given set of test assemblies, outputting the
    results to a file next to the original test assembly.
  .Description
    For each specified file, the results file is output to a file next
    to the original.  For instance, given an output format of xunit:

    Input File - c:\source\foo.tests.dll
    Output File - c:\source\nunit.TestResults.foo.tests.dll.xml

    If the output format is nunit, the results will be merged together
    so that they may be imported into a build server like Jenkins. The
    output file will automatically be placed in the first specified path
    and will be named, in the above instance, to the default

    c:\source\nunit.TestResults.xml

    If the XUnitPath is not specified, it will be resolved automatically
    by searching sibling directories.
  .Parameter Path
    A list of root paths to search through
  .Parameter TestSpec
    A wildcard specification of files to look for in each of the given
    paths.  This should follow the syntax that the -Include switch of
    Get-ChildItem accepts.

    Defaults to *Tests.dll
  .Parameter IncludeTraits
    XUnit traits to include when running tests. Specified as a Hashtable.

    @{Trait = Value; Trait2 = Value2; }

    When multiple traits are included, they pass through an OR filter.
  .Parameter ExcludeTraits
    XUnit traits to exclude when running tests. Specified as a Hashtable.

    @{Trait = Value; Trait2 = Value2; }

    When multiple traits are excluded, they pass through an AND filter.
  .Parameter OutputFormat
    The output format for XUnit - xunit, nunit or html.

    The default is nunit.  If nunit is specified, results will be merged
    and summarized into an additional xml file.
  .Parameter SummaryPath
    Only valid when using the nunit OutputFormat.  Overrides the default
    path generation algorithm for the merged output file.
  .Parameter XUnitPath
    The optional directory from which XUnit will be loaded.

    Must be xunit.console.clr4.exe, xunit.console.exe,
    xunit.console.clr4.x86.exe or xunit.console.x86.exe.

    If left unspecified, the sibling directories of the Midori package
    will be scanned for anything matching tools\xunit.console.clr4.exe,
    and the first matching file will be used if multiples are found.

    XUnit should be package restored by a scripted bootstrap process.
  .Example
    Invoke-XUnit -Path c:\source\foo, c:\source\bar

    Description
    -----------
    Will execute XUnit against all *Tests.dll assemblies found in
    c:\source\foo and c:\source\bar, outputting in the default nunit
    format.  Each found assembly will have a test results file placed
    next to it on disk. A merge of all the test runs, including summary
    data will be written to c:\source\foo\nunit.TestResults.xml
  .Example
    Invoke-XUnit -Path c:\src\foo -TestSpec '*Tests*.dll','*Runs*.dll' `
    -IncludeTraits @{Category=Unit} -ExcludeTraits @{Category=Smoke} `
    -SummaryPath c:\src\nunit.xml

    Description
    -----------
    Will execute XUnit against all *Tests*.dll and *Runs*.dll assemblies
    found in c:\src, outputting in the default nunit format. Each found
    assembly will have a test results file placed next to it on disk.
    A merge of all the test runs, including summary data will be written
    to c:\src\nunit.xml
  #>
  [CmdletBinding()]
  param(
    [Parameter(Mandatory=$true)]
    [string[]]
    [ValidateScript({(Test-Path $_) -and (Get-Item $_).PSIsContainer})]
    $Path,

    [Parameter(Mandatory=$false)]
    [string[]]
    $TestSpec = @('*Tests.dll'),

    [Parameter(Mandatory=$false)]
    [Hashtable]
    $IncludeTraits = @{},

    [Parameter(Mandatory=$false)]
    [Hashtable]
    $ExcludeTraits = @{},

    [Parameter(Mandatory=$false)]
    [string]
    [ValidateSet('xml','html','nunit')]
    $OutputFormat = 'nunit',

    [Parameter(Mandatory=$false)]
    [string]
    $SummaryPath = $null,

    [Parameter(Mandatory=$false)]
    [IO.FileInfo]
    [ValidateScript({
      (Test-Path $_) -and (!$_.PSIsContainer) -and
      ($_.Name -imatch $script:XunitRunnerSpec)
    })]
    $XUnitPath = (Get-XUnitPath)
  )

  if (! (Test-Path $XUnitPath))
    { throw "Could not find XUnit!  Restore with Nuget. "}
  if (($SummaryPath -ne $null) -and ($OutputFormat -ne 'nunit'))
    { throw "SummaryPath may only be specified with nunit OutputFormat" }

  $projectPath = New-XUnitProjectFile -Path $Path `
    -TestSpec $TestSpec -OutputFormat $OutputFormat

  $xargs = @($projectPath)
  $IncludeTraits.GetEnumerator() |
    % { $xargs += @("/trait","`"$($_.Key)=$($_.Value)`"")}
  $ExcludeTraits.GetEnumerator() |
    % { $xargs += @("/-trait", "`"$($_.Key)=$($_.Value)`"")}

  Write-Host "Invoking XUnit against $projectPath`n"
  &"$XUnitPath" $xargs

  if ($OutputFormat -eq 'nunit')
  {
    $xml = [xml](Get-Content $projectPath)
    $destination = if (-not [string]::IsNullOrEmpty($SummaryPath)) { $SummaryPath }
      else { Join-Path ($Path | Select -First 1) "$OutputFormat.TestResult.xml"}
    $params = @{
      Path = $xml.xunit.assemblies.assembly | % { $_.output.filename };
      Destination = $destination
    }
    New-MergedNUnitXml @params
  }
}

Export-ModuleMember -Function Set-XUnitPath, New-XUnitProjectFile, Invoke-XUnit,
  New-MergedNUnitXml
