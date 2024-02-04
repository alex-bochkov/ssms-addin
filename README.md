# Repository Archived
This repository is no longer active, as we have moved to a newer version. For the latest updates and features, please visit the new repository at [Axial SQL Tools](https://github.com/Axial-SQL/AxialSqlTools). We encourage you to check out the new version for the most up-to-date tools and improvements.

<hr/>
<h2>SQL Server Management Studio 2018 Productivity Tool (Addin)</h2>
<p>Work in progress... I had to make it as an "ugly" toolbox window because I couldn't figure out how to make a dynamic menu in a toolbar.</p>
<ul>
  <li>Format selected TSQL code with internal SQL Server parser or Poor Man's SQL Formatter</li>
  <li>Quick access to code templates</li>
</ul>

<p>.NET 4.7.2 is required.</p>

<p>To install the addin unpack files from the most recent <a href="https://github.com/alex-bochkov/ssms-addin/releases">release</a> into <i>C:\Program Files (x86)\Microsoft SQL Server Management Studio 18\Common7\IDE\Extensions</i> folder.</p>

<img src="https://github.com/alekseybochkov/ssms-addin/blob/master/Addin.SSMS2018/pics/1.png?raw=true"/>
<img src="https://github.com/alekseybochkov/ssms-addin/blob/master/Addin.SSMS2018/pics/2.png?raw=true"/>
<img src="https://github.com/alekseybochkov/ssms-addin/blob/master/Addin.SSMS2018/pics/3.png?raw=true"/>


<h2>(OLD) Simple addin for SQL Server Management Studio 2008-2014 that helps with:</h2>
<ul>
  <li>Export current GRID to Excel files (without re-execution!)</li>
  <li>Format selected TSQL code with internal SQL Server parser</li>
  <li>Quick access to code templates</li>
</ul>

<h2>Installation:</h2>
<ul>
  <li>Unpack files into any folder</li>
  <li>Modify SSMSTool.AddIn file - change path to SSMSTool.dll file</li>
  <li>Copy SSMSTool.AddIn file to <strong>C:\ProgramData\Application Data\Microsoft\MSEnvShared\Addins</strong> folder (create it if not exists)</li>
  <li>If error 80131515 occures - add &ltloadFromRemoteSources enabled="true" /&gt into "C:\Program Files (x86)\Microsoft SQL Server\120\Tools\Binn\ManagementStudio\Ssms.exe.config" right after &ltruntime&gt</li>
</ul>

<img src="https://github.com/alekseybochkov/ssms-addin/blob/master/screenshot.png?raw=true"/>

<p>SQL Server Management Studio 2016+ is not supported.</p>

