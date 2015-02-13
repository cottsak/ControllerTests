$startPath = "C:\projects\controllertests\ControllerTests.Tests\bin\Debug\"
$sqlInstance = "(local)\SQL2012SP1"
$dbName = "ControllerTests.Web"

# replace the db connection with the local instance 
$config = join-path $startPath "ControllerTests.Tests.dll.config"
$doc = (gc $config) -as [xml]
$doc.SelectSingleNode('//connectionStrings/add[@name="store"]').connectionString = "Server=$sqlInstance; Database=$dbName; Trusted_connection=true"
$doc.Save($config)

# attach mdf to local instance
$mdfFile = join-path $startPath "store.mdf"
$ldfFile = join-path $startPath "store_log.ldf"
sqlcmd -S "$sqlInstance" -Q "Use [master]; CREATE DATABASE [$dbName] ON (FILENAME = '$mdfFile'),(FILENAME = '$ldfFile') for ATTACH"
