// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Helper.js" company="RHEA System S.A.">
//    Copyright (c) 2022 RHEA System S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Jaime Bernar
//
//    This file is part of COMET WEB Community Edition
//    The COMET WEB Community Edition is the RHEA Web Application implementation of ECSS-E-TM-10-25 Annex A and Annex C.
//
//    The COMET WEB Community Edition is free software; you can redistribute it and/or
//    modify it under the terms of the GNU Affero General Public
//    License as published by the Free Software Foundation; either
//    version 3 of the License, or (at your option) any later version.
//
//    The COMET WEB Community Edition is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
//    Affero General Public License for more details.
//
//    You should have received a copy of the GNU Affero General Public License
//    along with this program.  If not, see <http://www.gnu.org/licenses/>.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

/**
 * Creates a babylon.js material from the specified colors. For more info of the colors: https://learnopengl.com/Lighting/Basic-Lighting
 * @param {Vector3} diffuse - the diffuse color
 * @param {Vector3} specular - the specular color.
 * @param {Vector3} emissive - the emissive color
 * @param {Vector3} ambient - the ambient global ilumination color.
 * @param {string} materialName - the name of the new material.
 * @param {BABYLON.js scene} scene - the scene to add the material to.
 * @returns {BABYLON.js Material} 
 */
function CreateMaterial(diffuse, specular, emissive, ambient, materialName, scene) {
    var babylonMaterial = new BABYLON.StandardMaterial(materialName, scene);
    babylonMaterial.diffuseColor = new BABYLON.Color3(diffuse.X, diffuse.Y, diffuse.Z);
    babylonMaterial.specularColor = new BABYLON.Color3(specular.X, specular.Y, specular.Z);
    babylonMaterial.emissiveColor = new BABYLON.Color3(emissive.X, emissive.Y, emissive.Z);
    babylonMaterial.ambientColor = new BABYLON.Color3(ambient.X, ambient.Y, ambient.Z);
    return babylonMaterial;
}

/**
 * Creates an skybox. Sky with texture for the background.
 * @param {BABYLON.js scene} scene - the scene to add the skybox to.
 * @param {number} size - the size of the scene.
 */
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

/**
 * Creates the picking material used in the scene.
 * @returns {BABYLON.js material} - the material to use.
 */
function SetUpPickingMaterial() {
    var pickingMaterial = new BABYLON.StandardMaterial("PickingMaterial", Scene);
    pickingMaterial.diffuseColor =  new BABYLON.Color3(0.8, 0.35, 0.35);
    pickingMaterial.specularColor = new BABYLON.Color3(1.0, 1.0, 1.0);
    pickingMaterial.emissiveColor = new BABYLON.Color3(0.15, 0.15, 0.15);
    pickingMaterial.ambientColor =  new BABYLON.Color3(0.5, 0.5, 0.5);
    return pickingMaterial;
}

/**
 * Calls a C# method that is in the COMETwebapp assembly. More Info: https://learn.microsoft.com/en-us/aspnet/core/blazor/javascript-interoperability/call-dotnet-from-javascript?view=aspnetcore-6.0
 * @param {string} methodName - the name of the method to invoke.
 * @returns {any} the same result as the invoked method.
 */
function CallCSharpMethod(methodName) {
    return DotNet.invokeMethod('COMETwebapp', methodName);
}

/**
 * Call the method to get GUIDs from C#
 */
function GetGUID() {
    return CallCSharpMethod('GetGUID');
}
