# ![Logo](docs/Media/logo.png) AzTestReporter
Summarize Azure DevOps test results into a HTML report format output to HTML file that includes, high-level summary of Test Runs & Code Coverage, summary details of Tests executed by Test class, Failure details if there are failures by Test class, summary of Code coverage results by module. 

## How to install


## Status

![Build Status](https://img.shields.io/azure-devops/build/HermesProjects/e8c0d705-6817-4252-acb1-5ec8ad488166/2)
![Test Status](https://img.shields.io/azure-devops/tests/HermesProjects/e8c0d705-6817-4252-acb1-5ec8ad488166/2)
![Code Coverage Status](https://img.shields.io/azure-devops/coverage/HermesProjects/e8c0d705-6817-4252-acb1-5ec8ad488166/2)

## Usage

### 1. Run the executable

### 2. Run via Azure Extensions

### 3. Enable in Build Pipeline

### 4. Enable in Release Pipeline

## High Level Architecture.

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

## Support

## Contributions
