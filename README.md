# ![Logo](AzTestReporter/docs/Media/logo.png) AzTestReporter
Summarize Azure DevOps test results into a HTML report format that is output to HTML file. The tool also has the capability to be configured to send a mail to the desired email's as part of execution.

## 1. Why?

## 2. Usage
Run the Executable from the command line, in Visual Studio or in an Azure Pipeline or Release.

### 2.1 Set Environment Variables for local run
For local run several Environment Variables will need to be set to enable run:

It is recommended to set up a powershell script to make this easier if not running in Visual Studio
$env:SYSTEM_ACCESSTOKEN = 'Your personal access token'
$env:BUILD_SOURCEBRANCH = 'Your branch for build pipeline in Azure DevOps'
$env:BUILD_DEFINITIONNAME = 'Build definition'
$env:SYSTEM_TEAMPROJECT = 'Team project in Azure DevOps'
$env:SYSTEM_DEFINITIONID = 'Definition ID'
$env:SYSTEM_TEAMFOUNDATIONCOLLECTIONURI = 'https://dev.azure.com/*your_organization'
$env:BUILD_REPOSITORY_NAME = 'Name of code repository'
$env:SYSTEM_ENABLEACCESSTOKEN = 'true'
$env:SYSTEM_TEAMFOUNDATIONSERVERURI = 'https://vsrm.dev.azure.com/*your_organization'
$env:BUILD_BUILDNUMBER = 'Build number'
$env:SYSTEM_HOSTTYPE = 'Release'

When running in Visual Studio debug create the following launchSettings.json under the properties for the AzTestReporter.App

{
    "profiles": {
        "AzTestReporter.App": {
            "commandName": "Project",
            "commandLineArgs": "--trt Unit --sendmail false --v true",
            "environmentVariables": {
                "SYSTEM_ACCESSTOKEN": "Your personal access token",
                "BUILD_SOURCEBRANCH": "Your branch for build pipeline in Azure DevOps",
                "BUILD_DEFINITIONNAME": "Build definition",
                "SYSTEM_TEAMPROJECT": "Team project in Azure DevOps",
                "SYSTEM_DEFINITIONID": "Definition ID",
                "SYSTEM_TEAMFOUNDATIONCOLLECTIONURI": "https://dev.azure.com/*your_organization",
                "BUILD_REPOSITORY_NAME": "Name of code repository",
                "SYSTEM_ENABLEACCESSTOKEN": "true",
                "SYSTEM_TEAMFOUNDATIONSERVERURI": "https://vsrm.dev.azure.com/*your_organization",
                "BUILD_BUILDNUMBER": "Build number",
                "SYSTEM_HOSTTYPE": "Release"
            }
        }
    }
}

### 2.2 Run the executable
In PowerShell or Command run the Executable "AzTestReporter.App.exe" using switch --help to display switch options

  --trt            Specify the test run type, supported types are - (Unit|Integration)

  --sendmail       (Default: true) Set this to false when debugging or when trying out the tool and not sending mail
                   inadvertently.

  --mailserver     The mail server to use when sending mail containing the test results.

  --mailaccount    The mail account to send mail.

  --mailpwd        The mail account password to use when sending mail.

  --sendto         List of people or groups mail needs to be sent to. This is a comma delimited list

  --cc             List of people or groups to cc in mail sent. This is a comma delimited list

  --help           Display this help screen.

  --version        Display version information.
  
  
### 2.3 Run via Azure Extensions

### 2.4 Enable in Build Pipeline

### 2.5 Enable in Release Pipeline

## 3. High Level Architecture.

## 4. Generated Reports
The tool supports generating reports for tests executed in both Build (Unit tests) and Release pipelines (Integration tests). In cases where there are failures encountered before the actual tests are executed, the tool outputs a Execution Failure report file. For quick samples of how the final reports would look, please view sample reports in the following links:

- [Build Test Report](AzTestReporter/docs/UnitTestResults-Example-TestExecutionReport.html)
- [Release Test Report](AzTestReporter/docs/)

For in depth details what the final report looks like, please go [here](AzTestReporter/docs/ReportDetails.md).


## 4. What's next.

1. View Execution Environment details.
2. Ability to support custom templates.
3. Output the datamodel into JSON for users to upload the json to datastore of their choice to enable data monitoring scenarios.
4. Code coverage reporting for Integration tests.
5. Object Model overview.

## 5. Support

## 6. Contributions
