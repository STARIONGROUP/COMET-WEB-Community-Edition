function CreateMaterial(diffuse, specular, emissive, ambient, materialName, scene) {
    var babylonMaterial = new BABYLON.StandardMaterial(materialName, scene);
    babylonMaterial.diffuseColor = new BABYLON.Color3(diffuse.X, diffuse.Y, diffuse.Z);
    babylonMaterial.specularColor = new BABYLON.Color3(specular.X, specular.Y, specular.Z);
    babylonMaterial.emissiveColor = new BABYLON.Color3(emissive.X, emissive.Y, emissive.Z);
    babylonMaterial.ambientColor = new BABYLON.Color3(ambient.X, ambient.Y, ambient.Z);
    return babylonMaterial;
}

function CreateSkybox(scene, size) {
    var skybox = BABYLON.MeshBuilder.CreateBox("skyBox", { size: size }, scene);
    skybox.CometID = "Skybox";
    var skyboxMaterial = new BABYLON.StandardMaterial("skyBox", scene);
    skyboxMaterial.backFaceCulling = false;
    skyboxMaterial.reflectionTexture = new BABYLON.CubeTexture("../Skybox/sky", scene);
    skyboxMaterial.reflectionTexture.coordinatesMode = BABYLON.Texture.SKYBOX_MODE;
    skyboxMaterial.diffuseColor = new BABYLON.Color3(0, 0, 0);
    skyboxMaterial.specularColor = new BABYLON.Color3(0, 0, 0);
    skybox.material = skyboxMaterial;
}

function SetUpPickingMaterial() {
    var pickingMaterial = new BABYLON.StandardMaterial("PickingMaterial", scene);
    pickingMaterial.diffuseColor =  new BABYLON.Color3(0.8, 0.35, 0.35);
    pickingMaterial.specularColor = new BABYLON.Color3(1.0, 1.0, 1.0);
    pickingMaterial.emissiveColor = new BABYLON.Color3(0.15, 0.15, 0.15);
    pickingMaterial.ambientColor =  new BABYLON.Color3(0.5, 0.5, 0.5);
    return pickingMaterial;
}



//---------------------------
//----- CALL C# FROM JS -----
//---------------------------


function CallCSharpMethod(methodName) {
    return DotNet.invokeMethod('COMETwebapp', methodName);
}

//Helper method to call a C# function that returns a GUID as JS dont have a method to create one
function GetGUID() {
    return CallCSharpMethod('GetGUID');
}

function GetDefaultMaterial(scene) {
    var data = CallCSharpMethod('GetDefaultMaterial');
    return TransformMaterialData(JSON.parse(data), scene);
}

