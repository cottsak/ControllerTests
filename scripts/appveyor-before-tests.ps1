$config = "C:\projects\controllertests\ControllerTests.Tests\bin\Debug\ControllerTests.Tests.dll.config"
$doc = (gc $config) -as [xml]
$doc.SelectSingleNode('//connectionStrings/add[@name="store"]').connectionString = 'Server=.\sqlexpress; Database=ControllerTests.Web; Trusted_connection=true'
$doc.Save($config)