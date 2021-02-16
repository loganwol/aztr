# ![Logo](docs/Media/logo.png) AzTestReporter
Summarize Azure DevOps test results into a HTML report format output to HTML file that includes, high-level summary of Test Runs & Code Coverage, summary details of Tests executed by Test class, Failure details if there are failures by Test class, summary of Code coverage results by module. 

## Status
![Build Status](https://img.shields.io/azure-devops/build/HermesProjects/e8c0d705-6817-4252-acb1-5ec8ad488166/2)
![Test Status](https://img.shields.io/azure-devops/tests/HermesProjects/e8c0d705-6817-4252-acb1-5ec8ad488166/2)
![Code Coverage Status](https://img.shields.io/azure-devops/coverage/HermesProjects/e8c0d705-6817-4252-acb1-5ec8ad488166/2)

## Releases
![Nuget Release](https://img.shields.io/nuget/v/aztestreporter)

## Usage
Run the Executable from the command line, in Visual Studio or in an Azure Pipeline or Release.

### 1. Set Environment Variables for local run
For a local run, several Environment Variables will need to be set to enable run the utility. *Important: This is only required if you're running locally, if you are running in Azure pipelines, you should download the project nuget package.* 

a. When running the exe standalone, please create a .bat file, copy paste the script below.
b. When running the exe from the solution, please create a launchSettings.json file for  AzTestReporter.App project. 

Suggestion: Sometimes it's hard to figure out all the parameters. If you've having difficulty figuring this out, refer to an existing Release pipeline or create one. 
If a Release pipeline exists, then go to one of the stages in the pipeline. Click on the Logs button when you hover over the stage and then click on the first task that's executed. It should be named Initialize job. Here you can search for each of the environment variables mentioned below and enter it. 

[Example Release pipeline](https://dev.azure.com/HermesProjects/AzureTestReports/_release?_a=releases&view=mine&definitionId=1)
1. Click on the latest Release.
2. Click on the first stage in the Release pipeline.
3. Click on Initialize job.
4. CTRL+F to search for the environment variables.

#### 1.1 Batch file samples

##### 1.1.a Build Pipeline Batch file

``` bat 
	set SYSTEM_ACCESSTOKEN='Your personal access token'
	set BUILD_SOURCEBRANCH='Your branch for build pipeline in Azure DevOps'
	set BUILD_DEFINITIONNAME='The desired Build definition name'
	set SYSTEM_TEAMPROJECT='Team project in Azure DevOps'
	set BUILD_DEFINITIONID='Definition ID'
	set SYSTEM_TEAMFOUNDATIONCOLLECTIONURI='https://dev.azure.com/*your_organization'
	set BUILD_REPOSITORY_NAME='Name of code repository'
	set SYSTEM_ENABLEACCESSTOKEN='true'
	set SYSTEM_TEAMFOUNDATIONSERVERURI='https://vsrm.dev.azure.com/*your_organization'
	set BUILD_BUILDNUMBER='Build number'
	set SYSTEM_HOSTTYPE='build'
```

##### 1.1.b Release Pipeline Batch file

```bat 
	set SYSTEM_ACCESSTOKEN='Your personal access token'
	set BUILD_SOURCEBRANCH='Your branch for build pipeline in Azure DevOps'
	set BUILD_DEFINITIONNAME='The desired Build definition name'
	set SYSTEM_TEAMPROJECT='Team project in Azure DevOps'
	set BUILD_DEFINITIONID='Definition ID'
	set SYSTEM_TEAMFOUNDATIONCOLLECTIONURI='https://dev.azure.com/*your_organization'
	set BUILD_REPOSITORY_NAME='Name of code repository'
	set SYSTEM_ENABLEACCESSTOKEN='true'
	set SYSTEM_TEAMFOUNDATIONSERVERURI='https://vsrm.dev.azure.com/*your_organization'
	set BUILD_BUILDNUMBER='Build number'
	set SYSTEM_HOSTTYPE='Release'
	set RELEASE_RELEASEID='The release ID'
	set RELEASE_DEFINITIONNAME=''
	set RELEASE_RELEASENAME=''
	set RELEASE_ENVIRONMENTNAME=''
	set RELEASE_ENVIRONMENTID=''
	set RELEASE_ATTEMPTNUMBER=''
	set AGENT_ID=''
```

When running in Visual Studio debug create the following launchSettings.json under the properties for the AzTestReporter.App.

#### 1.2 launchSettings.json file samples

##### 1.2.c Build Pipeline launchSettings.json file

```json
{
    "profiles": {
        "UniqueProfileName": {
            "commandName": "Project",
            "commandLineArgs": "--trt Unit --sendmail false",
            "environmentVariables": {
                "SYSTEM_ACCESSTOKEN": "Your personal access token",
                "BUILD_SOURCEBRANCH": "Your branch for build pipeline in Azure DevOps",
                "BUILD_DEFINITIONNAME": "Build definition",
                "SYSTEM_TEAMPROJECT": "Team project in Azure DevOps",
                "BUILD_DEFINITIONID": "Definition ID",
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
```

##### 1.2.d Release Pipeline launchSettings.json file

```json
{
  "profiles": {
    "UniqueProfileName": {
          "commandName": "Project",
          "commandLineArgs": "--trt Integration --sendmail false",
          "environmentVariables": {
            "SYSTEM_ACCESSTOKEN": "Your personal access token",
            "BUILD_SOURCEBRANCH": "Your branch for build pipeline in Azure DevOps",
            "BUILD_DEFINITIONNAME": "Build definition",
            "SYSTEM_TEAMPROJECT": "Team project in Azure DevOps",
            "BUILD_DEFINITIONID": "Definition ID",
            "SYSTEM_TEAMFOUNDATIONCOLLECTIONURI": "your_organization_devops_url",
            "BUILD_REPOSITORY_NAME": "Name of code repository",
            "SYSTEM_ENABLEACCESSTOKEN": "true",
            "SYSTEM_TEAMFOUNDATIONSERVERURI": "your_organization_devops_url starts with https://vsrm",
            "BUILD_BUILDNUMBER": "Build number",
            "SYSTEM_HOSTTYPE": "Release",
            "RELEASE_RELEASEID": "Release ID",
            "RELEASE_DEFINITIONNAME": "Release definition name",
            "RELEASE_RELEASENAME": "Release name",
            "RELEASE_ENVIRONMENTNAME": "Release environemt name",
            "RELEASE_ENVIRONMENTID": "Environment ID",
            "RELEASE_ATTEMPTNUMBER": "Attempt",
            "AGENT_ID": "Agent ID"
          }
        }
    }
}
```

### 1.3 Executable command line options
In PowerShell or Command run the Executable "AzTestReporter.App.exe" using switch --help to display switch options

| Command 				| Description 																		|
| --------------------- | --------------------------------------------------------------------------------- |
| --trt	  		| Specify the test run type, supported types are - (Unit|Integration) 					|
|  --sendmail 	| (Default: true) Set this to false when debugging or when trying out the tool and not sending mail inadvertently. |
| --mailserver 	| The mail server to use when sending mail containing the test results.|
| --mailaccount | The mail account to send mail. |
| --mailpwd 	| 	The mail account password to use when sending mail. |
| --sendto 		|  List of people or groups mail needs to be sent to. This is a comma delimited list. |
| --cc 			| List of people or groups to cc in mail sent. This is a comma delimited list |
|  --o 			| The directory where the report file will be output to. |
|  --help 		| Display this help screen. |
|  --version 	| Display version information. |
  
  
### 2. Run via Azure Extensions
Coming soon.

### 3. Enable in Build Pipeline
Coming soon.

### 4. Enable in Release Pipeline
Coming soon.

## High Level Architecture.
Coming soon.

## Generated Reports
The tool supports generating reports for tests executed in both Build (Unit tests) and Release pipelines (Integration tests). In cases where there are failures encountered before the actual tests are executed, the tool outputs a Execution Failure report file. For quick samples of how the final reports would look, please view sample reports in the following links:

- [Build Test Report](https://loganwol.github.io/aztr/UnitTestResults-Example-TestExecutionReport.html)
- [Release Test Report](docs/)
- [Failures Report](https://loganwol.github.io/aztr/ExecutionFailuresReport-Attempt0-1.1.37.html) 

For in depth details what the final report looks like, please go [here](docs/ReportDetails.md).


## What's next.

1. View Execution Environment details.
2. Ability to support custom templates.
3. Output the datamodel into JSON for users to upload the json to datastore of their choice to enable data monitoring scenarios.
4. Code coverage reporting for Integration tests.
5. Object Model overview.
6. .netcore support.

## Support

## Contributions
