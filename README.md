# ![Logo](docs/Media/logo.png) AzTestReporter
Summarize Azure DevOps test results into a HTML report format output to HTML file that includes, high-level summary of Test Runs & Code Coverage, summary details of Tests executed by Test class, Failure details if there are failures by Test class, summary of Code coverage results by module. 


## 1. How to install

## 2. Usage

### 2.1 Run the executable

### 2.2 Run via Azure Extensions

### 2.3 Enable in Build Pipeline

### 2.4 Enable in Release Pipeline

## 3. High Level Architecture.

## 4. Generated Reports
The tool supports generating reports for tests executed in both Build (Unit tests) and Release pipelines (Integration tests). In cases where there are failures encountered before the actual tests are executed, the tool outputs a Execution Failure report file. For quick samples of how the final reports would look, please view sample reports in the following links:

- [Build Test Report](docs/UnitTestResults-Example-TestExecutionReport.html)
- [Release Test Report](docs/)

For in depth details what the final report looks like, please go [here](docs/ReportDetails.md).


## 4. What's next.

1. View Execution Environment details.
2. Ability to support custom templates.
3. Output the datamodel into JSON for users to upload the json to datastore of their choice to enable data monitoring scenarios.
4. Code coverage reporting for Integration tests.
5. Object Model overview.

## 5. Support

## 6. Contributions
