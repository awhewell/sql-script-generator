[Setup]
AppName=SqlScriptGenerator
AppVerName=SqlScriptGenerator 1.0.0
ChangesEnvironment=true
DefaultDirName={pf}\SQLScriptGen
DefaultGroupName=SQL Script Generator
DisableDirPage=no
InfoBeforeFile=VersionHistory.txt
LicenseFile=..\LICENSE
OutputBaseFileName=SqlScriptGeneratorSetup

[Messages]
InfoBeforeLabel=Version History

[Tasks]
Name: modifypath; Description: &Add application directory to your system path; Flags: checkedonce

[Dirs]
Name: "{app}\bin"
Name: "{app}\bin\roslyn"

[Files]
Source: "..\LICENSE"; DestDir: "{app}"; Flags: ignoreversion;
Source: "..\SqlScriptGenerator\bin\Release\Dapper.dll"; DestDir: "{app}"; Flags: ignoreversion;
Source: "..\SqlScriptGenerator\bin\Release\Microsoft.CodeDom.Providers.DotNetCompilerPlatform.dll"; DestDir: "{app}"; Flags: ignoreversion;
Source: "..\SqlScriptGenerator\bin\Release\Newtonsoft.Json.dll"; DestDir: "{app}"; Flags: ignoreversion;
Source: "..\SqlScriptGenerator\bin\Release\SqlScriptGenerator.exe"; DestDir: "{app}"; Flags: ignoreversion;
Source: "..\SqlScriptGenerator\bin\Release\SqlScriptGenerator.exe.config"; DestDir: "{app}"; Flags: ignoreversion;
Source: "..\SqlScriptGenerator\bin\Release\bin\roslyn\*"; DestDir: "{app}\bin\roslyn"; Flags: ignoreversion;

[Code]
const
  	ModPathName = 'modifypath';
	  ModPathType = 'system';

function ModPathDir(): TArrayOfString;
begin
  	setArrayLength(Result, 1);
	  Result[0] := ExpandConstant('{app}');
end;
#include "modpath.iss"
