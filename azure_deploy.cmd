set basename=wepaddemo1
set RGNAME=%basename%rg
set arbitername=%basename%arbiter
set frontendname=%basename%frontend
set storename=%basename%store
set arbiterkey=thisasdfslkhjkfhkj2142rfKLJHLKHJCZgsdfg.XHFQ435GSLKVHJSLJKGHF

call az group create --name %RGNAME% --location uksouth

cd configArbiter
call az webapp up --name %arbitername% --resource-group %RGNAME% --sku p0v3 --os-type windows --runtime dotnet:8 --location uksouth
call az webapp config appsettings set --name %arbitername% --resource-group %RGNAME% --settings ARBITER_KEY=%arbiterkey%

cd ..
cd FrontendRouter
call az webapp up --name %frontendname% --resource-group %RGNAME% --sku p1v3 --os-type windows --runtime dotnet:8% --location uksouth
call az webapp config appsettings set --name %frontendname% --resource-group %RGNAME% --settings ARBITER_KEY=%arbiterkey%
call az webapp config appsettings set --name %frontendname% --resource-group %RGNAME% --settings ARBITER_ENDPOINT=https://%arbitername%.azurewebsites.net

cd .. 
cd Store
call az webapp up --name %storename%0-63 --resource-group %RGNAME% --sku p1v3 --os-type windows --runtime dotnet:8  --location uksouth
call az webapp config appsettings set --name  %storename%0-63 --resource-group %RGNAME% --settings FIXED_PARTITION=00:63
call az webapp config appsettings set --name  %storename%0-63 --resource-group %RGNAME% --settings ARBITER_KEY=%arbiterkey%
call az webapp config appsettings set --name  %storename%0-63 --resource-group %RGNAME% --settings ARBITER_ENDPOINT=https://%arbitername%.azurewebsites.net
call az webapp config appsettings set --name  %storename%0-63 --resource-group %RGNAME% --settings PARTITION_ENDPOINT=https://%frontendname%.azurewebsites.net

call az webapp up --name %storename%64-127 --resource-group %RGNAME% --sku p1v3 --os-type windows --runtime dotnet:8  --location uksouth
call az webapp config appsettings set --name %storename%64-127 --resource-group %RGNAME% --settings FIXED_PARTITION=64:127
call az webapp config appsettings set --name %storename%64-127 --resource-group %RGNAME% --settings ARBITER_KEY=%arbiterkey%
call az webapp config appsettings set --name %storename%64-127 --resource-group %RGNAME% --settings ARBITER_ENDPOINT=https://%arbitername%.azurewebsites.net
call az webapp config appsettings set --name %storename%64-127 --resource-group %RGNAME% --settings PARTITION_ENDPOINT=https://%frontendname%.azurewebsites.net

call az webapp up --name %storename%128-191 --resource-group %RGNAME% --sku p1v3 --os-type windows --runtime dotnet:8  --location uksouth
call az webapp config appsettings set --name %storename%128-191 --resource-group %RGNAME% --settings FIXED_PARTITION=128:191
call az webapp config appsettings set --name %storename%128-191 --resource-group %RGNAME% --settings ARBITER_KEY=%arbiterkey%
call az webapp config appsettings set --name %storename%128-191 --resource-group %RGNAME% --settings ARBITER_ENDPOINT=https://%arbitername%.azurewebsites.net
call az webapp config appsettings set --name %storename%128-191 --resource-group %RGNAME% --settings PARTITION_ENDPOINT=https://%frontendname%.azurewebsites.net

call az webapp up --name %storename%192-255 --resource-group %RGNAME% --sku p1v3 --os-type windows --runtime dotnet:8  --location uksouth
call az webapp config appsettings set --name %storename%192-255 --resource-group %RGNAME% --settings FIXED_PARTITION=192:255
call az webapp config appsettings set --name %storename%192-255 --resource-group %RGNAME% --settings ARBITER_KEY=%arbiterkey%
call az webapp config appsettings set --name %storename%192-255 --resource-group %RGNAME% --settings ARBITER_ENDPOINT=https://%arbitername%.azurewebsites.net
call az webapp config appsettings set --name %storename%192-255 --resource-group %RGNAME% --settings PARTITION_ENDPOINT=https://%frontendname%.azurewebsites.net

cd ..
