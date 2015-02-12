# replace the db connection with the local instance 
$startPath = "C:\projects\controllertests\ControllerTests.Tests\bin\Debug\"
$config = join-path $startPath "ControllerTests.Tests.dll.config"
$doc = (gc $config) -as [xml]
$doc.SelectSingleNode('//connectionStrings/add[@name="store"]').connectionString = 'Server=.\sqlexpress; Database=ControllerTests.Web; Trusted_connection=true'
$doc.Save($config)

# attach mdf to local instance
$mdfFile = join-path $startPath "store.mdf"
$ldfFile = join-path $startPath "store_log.ldf"
sqlcmd -S .\SQL2014 -U sa -P Password12! -Q "sp_attach_db 'ControllerTests.Web', '$mdfFile', '$ldfFile'"
