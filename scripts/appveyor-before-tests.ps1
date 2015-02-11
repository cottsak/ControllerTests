$config = "C:\projects\controllertests\ControllerTests.Tests\App.config"
$doc = (gc $config) -as [xml]
$doc.SelectSingleNode('//connectionStrings/add[@name="store"]/@connectionstring').'#text' = 'Server=.\sqlexpress; Database=ControllerTests.Web; Trusted_connection=true'
$doc.Save($config)