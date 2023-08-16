// --------------------------------------------------------------------------------------------------------------------
// <copyright file="babylonSpecifics.js" company="RHEA System S.A.">
//    Copyright (c) 2023 RHEA System S.A.
//
//    Authors: Sam Gerené, Alex Vorobiev, Alexander van Delft, Jaime Bernar
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
 * Creates a babylon.js scene.
 * @param {any} engine - the babylon.js engine to attach the canvas to.
 * @param {HTMLElement} canvas - the HTML5 Canvas element.
 */
function CreateScene(engine, canvas) {
    const scene = new BABYLON.Scene(engine);
    scene.clearColor = new BABYLON.Color3(0.98, 0.98, 0.98);

    Camera = new BABYLON.ArcRotateCamera("Camera", 0, 0, 10, new BABYLON.Vector3(0, 0, 0), scene);
    Camera.setPosition(new BABYLON.Vector3(20, 10, -30));
    Camera.attachControl(canvas, true);
    Camera.lowerRadiusLimit = 5;
    Camera.upperRadiusLimit = SkyboxSize / 2.0;
    Camera.inertia = CameraInertia;
    Camera.panningInertia = CameraInertia;
    Camera.angularSensibilityX = Camera.angularSensibilityY = CameraRotationSensibility;
    Camera.panningSensibility = CameraPanningSensibility;
    Camera.wheelPrecision = CameraZoomSensibility;

    let light = new BABYLON.HemisphericLight("HemisphericLight", new BABYLON.Vector3(2, 1, 0));
    scene.light = light;

    return scene;
};

/**
 * Register the mesh so it can be selected by the mouse cursor in real time.
 * @param {BABYLON.js mesh} mesh - the mesh to register.
 */
function RegisterMeshActions(mesh) {
    mesh.actionManager.registerAction(new BABYLON.SetValueAction(BABYLON.ActionManager.OnPointerOutTrigger, mesh.material, "emissiveColor", mesh.material.emissiveColor));
    mesh.actionManager.registerAction(new BABYLON.SetValueAction(BABYLON.ActionManager.OnPointerOverTrigger, mesh.material, "emissiveColor", PickingMaterial.emissiveColor));
    mesh.actionManager.registerAction(new BABYLON.SetValueAction(BABYLON.ActionManager.OnPointerOutTrigger, mesh.material, "diffuseColor", mesh.material.diffuseColor));
    mesh.actionManager.registerAction(new BABYLON.SetValueAction(BABYLON.ActionManager.OnPointerOverTrigger, mesh.material, "diffuseColor", PickingMaterial.diffuseColor));
    mesh.actionManager.registerAction(new BABYLON.SetValueAction(BABYLON.ActionManager.OnPointerOutTrigger, mesh.material, "specularColor", mesh.material.specularColor));
    mesh.actionManager.registerAction(new BABYLON.SetValueAction(BABYLON.ActionManager.OnPointerOverTrigger, mesh.material, "specularColor", PickingMaterial.specularColor));
    mesh.actionManager.registerAction(new BABYLON.SetValueAction(BABYLON.ActionManager.OnPointerOutTrigger, mesh.material, "ambientColor", mesh.material.ambientColor));
    mesh.actionManager.registerAction(new BABYLON.SetValueAction(BABYLON.ActionManager.OnPointerOverTrigger, mesh.material, "ambientColor", PickingMaterial.ambientColor));
}

/**
 * Creates a line primitive
 * @param {any} primitive - the primitive in JSON format
 * @param {any} color - the color in JSON format
 */
function CreateLine(primitive) {
    const lpoints = [
        new BABYLON.Vector3(primitive.P0.X, primitive.P0.Y, primitive.P0.Z),
        new BABYLON.Vector3(primitive.P1.X, primitive.P1.Y, primitive.P1.Z)
    ];
    let line = BABYLON.MeshBuilder.CreateLines("lines", { points: lpoints }, Scene);
    line.color = new BABYLON.Color3(primitive.Color.X, primitive.Color.Y, primitive.Color.Z);
    return line;
}

/**
 * Creates a box primitive
 * @param {any} primitive - the primitive in JSON format
 * @param {any} color - the color in JSON format
 */
function CreateBox(primitive) {
    let mesh = BABYLON.MeshBuilder.CreateBox("box", { width: primitive.Width, height: primitive.Height, depth: primitive.Depth }, Scene);
    return mesh;
}

/**
 * Creates a sphere primitive
 * @param {any} primitive - the primitive in JSON format
 * @param {any} color - the color in JSON format
 */
function CreateSphere(primitive) {
    let mesh = BABYLON.MeshBuilder.CreateSphere("sphere", { diameter: primitive.Radius * 2.0 }, Scene);
    return mesh;
}

/**
 * Creates a cylinder primitive
 * @param {any} primitive - the primitive in JSON format
 * @param {any} color - the color in JSON format
 */
function CreateCylinder(primitive) {
    let mesh = BABYLON.MeshBuilder.CreateCylinder("cylinder", { diameter: primitive.Radius * 2.0, height: primitive.Height }, Scene);
    return mesh;
}

/**
 * Creates a cone primitive
 * @param {any} primitive - the primitive in JSON format
 * @param {any} color - the color in JSON format
 */
function CreateCone(primitive) {
    let mesh = BABYLON.MeshBuilder.CreateCylinder("cone", { diameterTop: 0, diameterBottom: primitive.Radius * 2.0, height: primitive.Height, tessellation: 36 }, Scene);
    return mesh;
}

/**
 * Creates a torus primitive
 * @param {any} primitive - the primitive in JSON format
 * @param {any} color - the color in JSON format
 */
function CreateTorus(primitive) {
    let mesh = BABYLON.MeshBuilder.CreateTorus("torus", { diameter: primitive.Diameter, thickness: primitive.Thickness, tessellation: 36 }, Scene);
    return mesh;
}

/**
 * Creates a triangular prism primitive
 * @param {any} primitive - the primitive in JSON format
 * @param {any} color - the color in JSON format
 */
function CreateTriangularPrism(primitive) {
    let mesh = BABYLON.MeshBuilder.CreateCylinder("cylinder", { height: primitive.Height, diameter: primitive.Radius * 2.0, tessellation: 3 }, Scene);
    return mesh;
}

/**
 * Creates a hexagonal prism primitive
 * @param {any} primitive - the primitive in JSON format
 * @param {any} color - the color in JSON format
 */
function CreateHexagonalPrism(primitive) {
    let mesh = BABYLON.MeshBuilder.CreateCylinder("cylinder", { height: primitive.Height, diameter: primitive.Radius * 2.0, tessellation: 6 }, Scene);
    return mesh;
}

/**
 * Creates a disc primitive
 * @param {any} primitive - the primitive in JSON format
 * @param {any} color - the color in JSON format
 */
function CreateDisc(primitive) {
    let mesh = BABYLON.MeshBuilder.CreateCylinder("cylinder", { diameter: primitive.Radius * 2.0, height: primitive.Radius/100.0 }, Scene);
    return mesh;
}

/**
 * Creates a rectangle primitive
 * @param {any} primitive - the primitive in JSON format
 * @param {any} color - the color in JSON format
 */
function CreateRectangle(primitive) {
    let minValue = Math.min(primitive.Width, primitive.Height);
    let mesh = BABYLON.MeshBuilder.CreateBox("box", { width: primitive.Width, height: minValue / 100.0, depth: primitive.Height }, Scene);
    return mesh;
}

/**
 * Creates a triangle primitive
 * @param {any} primitive - the primitive in JSON format
 * @param {any} color - the color in JSON format
 */
function CreateTriangle(primitive) {
    let mesh = BABYLON.MeshBuilder.CreateCylinder("cylinder", { height: primitive.Radius/100.0, diameter: primitive.Radius * 2.0, tessellation: 3 }, Scene);
    return mesh;
}

/**
 * Creates a custom primitive
 * @param {any} primitive - the primitive in JSON format
 * @param {any} color - the color in JSON format
 */
async function LoadPrimitive(primitive) {
    let path = primitive.Path;
    let fileName = primitive.FileName;

    const result = await BABYLON.SceneLoader.ImportMeshAsync(null, path, fileName, Scene);
    let meshes = result.meshes;

    for (let mesh of meshes) {
        InitializePrimitiveData(mesh, primitive);
    }
}

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
    let babylonMaterial = new BABYLON.StandardMaterial(materialName, scene);
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
    let skybox = BABYLON.MeshBuilder.CreateBox("skyBox", { size: size }, scene);
    skybox.CometID = "Skybox";
    let skyboxMaterial = new BABYLON.StandardMaterial("skyBox", scene);
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
    let pickingMaterial = new BABYLON.StandardMaterial("PickingMaterial", Scene);
    pickingMaterial.diffuseColor = new BABYLON.Color3(0.8, 0.35, 0.35);
    pickingMaterial.specularColor = new BABYLON.Color3(1.0, 1.0, 1.0);
    pickingMaterial.emissiveColor = new BABYLON.Color3(0.15, 0.15, 0.15);
    pickingMaterial.ambientColor = new BABYLON.Color3(0.5, 0.5, 0.5);
    return pickingMaterial;
}
