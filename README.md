<div style="display: block;width: 100%;">
	<div style="clear: both;">
		<div style="float: left;height: 60px;overflow: hidden;padding: 3px 1.8%;">
			<img src="AzTestReporter/docs/Media/logo.png" alt="AzTR" style="width:50px"/>
		</div>
		<div style="float: left;height: 60px;overflow: hidden;padding: 3px 1.8%;width:82%;font-style:bold;font-size:xx-large;vertical-align:middle;padding-top:20px">
		AzTestReporter
		</div>
	</div>
</div>

<br/>
<hr/>

Summarize Azure DevOps test results into a HTML report format that is output to HTML file. The tool also has the capability to be configured to send a mail to the desired email's as part of execution.

## 1. Why?

## 2. Usage

### 2.1 Run the executable

### 2.2 Run via Azure Extensions

### 2.3 Enable in Build Pipeline

### 2.4 Enable in Release Pipeline

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