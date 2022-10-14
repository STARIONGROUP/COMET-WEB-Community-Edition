// --------------------------------------------------------------------------------------------------------------------
// <copyright file="babylonInterop.js" company="RHEA System S.A.">
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

/// <summary>
/// Skybox size, used for the cube that surrounds the scene.
/// </summary>
const SkyboxSize = 1400.0;

/// <summary>
/// Camera rotation sensibility. A higher number means that the angle of rotation per mouse displacement is fewer.
/// </summary>
const CameraRotationSensibility = 900.0;

/// <summary>
/// Camera panning sensibility. A higher number means that the panning distance per mouse displacement is fewer.
/// </summary>
const CameraPanningSensibility = 100;

/// <summary>
/// Camera zoom sensibility. A higher number means that the zoom per mouse wheel displacement is fewer.
/// </summary>
const CameraZoomSensibility = 0.3;

/// <summary>
/// The Inertia of the camera. Used to interpolate animations.
/// </summary>
const CameraInertia = 0.1;

/// <summary>
/// The Babylon.JS Scene
/// </summary>
var Scene;

/// <summary>
/// A list of the primitives that the scene contains.
/// </summary>
var Primitives = new Map();

/// <summary>
/// The material used when a primitive is picked.
/// </summary>
var PickingMaterial;

/// <summary>
/// The panel used for displaying primitive's details in the window.
/// </summary>
var DetailsPanel;

/// <summary>
/// Scene specular color. More Info: https://learnopengl.com/Lighting/Basic-Lighting
/// </summary>
var SceneSpecularColor;

/// <summary>
/// Scene emissive color. More Info: https://learnopengl.com/Lighting/Basic-Lighting
/// </summary>
var SceneEmissiveColor;

/// <summary>
/// Scene ambient color. More Info: https://learnopengl.com/Lighting/Basic-Lighting
/// </summary>
var SceneAmbientColor;

/// <summary>
/// Inits the babylon.js canvas, the asociated resources and starts the render loop.
/// </summary>
function InitCanvas() {
    var BabylonCanvas = document.getElementById("babylon-canvas");
    var BabylonEngine = new BABYLON.Engine(BabylonCanvas, true, { stencil: true, antialias: true });
    Scene = CreateScene(BabylonEngine, BabylonCanvas);
    CreateSkybox(Scene, SkyboxSize);

    PickingMaterial = SetUpPickingMaterial();
    DetailsPanel = document.getElementById("detailsPanel");

    SceneSpecularColor = new BABYLON.Color3(1.0, 1.0, 1.0);
    SceneEmissiveColor = new BABYLON.Color3(0.0, 0.0, 0.0);
    SceneAmbientColor  = new BABYLON.Color3(1.0, 1.0, 1.0);

    BabylonEngine.runRenderLoop(function () {
        Scene.render();
    });

    window.addEventListener("resize", function () {
        BabylonEngine.resize();
    });
};

/// <summary>
/// Creates the babylon.js scene.
/// </summary>
/// <param name="engine">the babylon.js engine</param>
/// <param name="canvas">the HTML5 canvas to attach the scene</param>
/// <returns>The babylon.js scene</returns>
function CreateScene (engine, canvas) {
    var scene = new BABYLON.Scene(engine);
    scene.clearColor = new BABYLON.Color3(0.98, 0.98, 0.98);

    var camera = new BABYLON.ArcRotateCamera("Camera", 0, 0, 10, new BABYLON.Vector3(0, 0, 0), scene);
    camera.setPosition(new BABYLON.Vector3(200, 100, -300));
    camera.attachControl(canvas, true);
    camera.lowerRadiusLimit = 5;
    camera.upperRadiusLimit = SkyboxSize / 2.0;
    camera.inertia = CameraInertia;
    camera.panningInertia = CameraInertia;
    camera.angularSensibilityX = camera.angularSensibilityY = CameraRotationSensibility;
    camera.panningSensibility = CameraPanningSensibility;
    camera.wheelPrecision = CameraZoomSensibility;
   
    var light1 = new BABYLON.HemisphericLight("HemisphericLight", new BABYLON.Vector3(2, 1, 0), scene);
    
    return scene;
};

/// <summary>
/// Gets the ID of the primitive that is under the mouse cursor.
/// </summary>
/// <returns>The ID of the primitive or null if there is no primitive under mouse cursor</returns>
function GetPrimitiveIDUnderMouse() {
    var hit = Scene.pick(Scene.pointerX, Scene.pointerY)
    var pickedMesh = hit.pickedMesh;

    if (pickedMesh.CometID != "Skybox")
    {
        return pickedMesh.CometID;
    }

    return null;
}

/// <summary>
/// Registers the mesh so it can be selected by the mouse in real time.
/// </summary>
/// <param name="mesh">the mesh to register</param>
function RegisterMeshActions(mesh) {
    mesh.actionManager.registerAction(new BABYLON.SetValueAction(BABYLON.ActionManager.OnPointerOutTrigger,  mesh.material, "emissiveColor", mesh.material.emissiveColor));
    mesh.actionManager.registerAction(new BABYLON.SetValueAction(BABYLON.ActionManager.OnPointerOverTrigger, mesh.material, "emissiveColor", PickingMaterial.emissiveColor));
    mesh.actionManager.registerAction(new BABYLON.SetValueAction(BABYLON.ActionManager.OnPointerOutTrigger,  mesh.material, "diffuseColor",  mesh.material.diffuseColor));
    mesh.actionManager.registerAction(new BABYLON.SetValueAction(BABYLON.ActionManager.OnPointerOverTrigger, mesh.material, "diffuseColor",  PickingMaterial.diffuseColor));
    mesh.actionManager.registerAction(new BABYLON.SetValueAction(BABYLON.ActionManager.OnPointerOutTrigger,  mesh.material, "specularColor", mesh.material.specularColor));
    mesh.actionManager.registerAction(new BABYLON.SetValueAction(BABYLON.ActionManager.OnPointerOverTrigger, mesh.material, "specularColor", PickingMaterial.specularColor));
    mesh.actionManager.registerAction(new BABYLON.SetValueAction(BABYLON.ActionManager.OnPointerOutTrigger,  mesh.material, "ambientColor",  mesh.material.ambientColor));
    mesh.actionManager.registerAction(new BABYLON.SetValueAction(BABYLON.ActionManager.OnPointerOverTrigger, mesh.material, "ambientColor",  PickingMaterial.ambientColor));
}

/// <summary>
/// Adds the primitive with the specified color to the scene.
/// </summary>
/// <param name="primitive">a JSON string representation of the primitive</param>
/// <param name="color">a JSON string representation of the color</param>
function AddPrimitive(primitive, color) {
    //Convert to JSON the parameters
    primitive = JSON.parse(primitive);
    color = JSON.parse(color);

    if (!primitive.hasOwnProperty("Type")) {
        throw "The parameter don't have the Type property, make sure that the object is of type Primitive and that Type property is overriden";
    }

    switch (primitive.Type) {
        case "Line":            CreateLine(primitive, color);     break;
        case "Cube":            CreateBox(primitive, color);      break;
        case "Sphere":          CreateSphere(primitive, color);   break;
        case "Cylinder":        CreateCylinder(primitive, color); break;
        case "Cone":            CreateCone(primitive, color);     break;
        case "Torus":           CreateTorus(primitive, color);    break;
        case "CustomPrimitive": LoadPrimitive(primitive, color);  break;

        default: throw "The type of the primitive is not defined in the JS file";
    }
}

/// <summary>
/// Creates a line primitive.
/// </summary>
/// <param name="primitive">the primitive in JSON format</param>
/// <param name="color">the color in JSON format</param>
function CreateLine(primitive, color) {
    const lpoints = [
        new BABYLON.Vector3(primitive.P0.X, primitive.P0.Y, primitive.P0.Z),
        new BABYLON.Vector3(primitive.P1.X, primitive.P1.Y, primitive.P1.Z)
    ];
    var line = BABYLON.MeshBuilder.CreateLines("lines", { points: lpoints }, Scene);
    line.color = new BABYLON.Color3(color.X, color.Y, color.Z);
}

/// <summary>
/// Creates a box primitive.
/// </summary>
/// <param name="primitive">the primitive in JSON format</param>
/// <param name="color">the color in JSON format</param>
function CreateBox(primitive, color) {
    var mesh = BABYLON.MeshBuilder.CreateBox("box", { width: primitive.Width, height: primitive.Height, depth: primitive.Depth }, Scene);
    InitializePrimitiveData(mesh, primitive, color);
}

/// <summary>
/// Creates a sphere primitive.
/// </summary>
/// <param name="primitive">the primitive in JSON format</param>
/// <param name="color">the color in JSON format</param>
function CreateSphere(primitive, color) {
    var mesh = BABYLON.MeshBuilder.CreateSphere("sphere", { diameter: primitive.Radius * 2.0 }, Scene);
    InitializePrimitiveData(mesh, primitive, color);
}

/// <summary>
/// Creates a cylinder primitive.
/// </summary>
/// <param name="primitive">the primitive in JSON format</param>
/// <param name="color">the color in JSON format</param>
function CreateCylinder(primitive, color) {
    var mesh = BABYLON.MeshBuilder.CreateCylinder("cone", { diameter: primitive.Radius * 2.0, height: primitive.Height }, Scene);
    InitializePrimitiveData(mesh, primitive, color);
}

/// <summary>
/// Creates a cone primitive.
/// </summary>
/// <param name="primitive">the primitive in JSON format</param>
/// <param name="color">the color in JSON format</param>
function CreateCone(primitive, color) {
    var mesh = BABYLON.MeshBuilder.CreateCylinder("cone", { diameterTop: 0, diameterBottom: primitive.Radius * 2.0, height: primitive.Height }, Scene);
    InitializePrimitiveData(mesh, primitive, color);
}

/// <summary>
/// Creates a torus primitive.
/// </summary>
/// <param name="primitive">the primitive in JSON format</param>
/// <param name="color">the color in JSON format</param>
function CreateTorus(primitive, color) {
    var mesh = BABYLON.MeshBuilder.CreateTorus("torus", { diameter: primitive.Diameter, thickness: primitive.Thickness, tessellation : 36 }, Scene);
    InitializePrimitiveData(mesh, primitive, color);
}

/// <summary>
/// Creates a custom primitive.
/// </summary>
/// <param name="primitive">the primitive in JSON format</param>
/// <param name="color">the color in JSON format</param>
async function LoadPrimitive(primitive, color) {
    var path = primitive.Path;
    var fileName = primitive.FileName;

    const result = await BABYLON.SceneLoader.ImportMeshAsync(null, path, fileName, Scene);
    var meshes = result.meshes;

    for (let i = 0; i < meshes.length; i++) {
        InitializePrimitiveData(meshes[i], primitive, color);
    }
}

/// <summary>
/// Initializes and creates important data for the primitives
/// </summary>
/// <param name="mesh">the babylon.js mesh</param>
/// <param name="primitive">the primitive in JSON format</param>
/// <param name="color">the color in JSON format</param>
function InitializePrimitiveData(mesh, primitive, color) {

    if (primitive.hasOwnProperty("Subtype") && primitive.Subtype == "Positionable") {
        mesh.position.x = primitive.X;
        mesh.position.y = primitive.Y;
        mesh.position.z = primitive.Z;

        mesh.rotation.x = primitive.RX;
        mesh.rotation.y = primitive.RY;
        mesh.rotation.z = primitive.RZ;
    }

    var babylonMaterial = CreateMaterial(color, SceneSpecularColor, SceneEmissiveColor, SceneAmbientColor, "DefaultMaterial", Scene);
    mesh.material = babylonMaterial;

    mesh.actionManager = new BABYLON.ActionManager(Scene);
    RegisterMeshActions(mesh);

    //Custom properties for the object
    mesh.CometID = primitive.ID;
    mesh.Materials = [mesh.material, PickingMaterial];

    Primitives.set(primitive.ID, { "mesh": mesh, "primitive": primitive });
}

/// <summary>
/// Removes the primitive with id from the scene.
/// </summary>
/// <param name="Id">the Id of the primitive to delete</param>
function Dispose(Id) {
    var data = Primitives.get(Id);
    var mesh = data["mesh"];
    if (mesh != null) {
        mesh.dispose();
    }
}

/// <summary>
/// Sets the translation of the primitive with id.
/// </summary>
/// <param name="Id">the Id of the primitive to delete</param>
/// <param name="x">the translation along the x axis</param>
/// <param name="y">the translation along the y axis</param>
/// <param name="z">the translation along the z axis</param>
function SetTranslation(Id, x, y, z) {    
    var data = Primitives.get(Id);
    if (data != undefined) {
        var mesh = data["mesh"];
        mesh.position.x = x;
        mesh.position.y = y;
        mesh.position.z = z;
    }
}

/// <summary>
/// Sets the rotation of the primitive with id.
/// </summary>
/// <param name="Id">the Id of the primitive to delete</param>
/// <param name="x">the rotation around the x axis</param>
/// <param name="y">the rotation around the y axis</param>
/// <param name="z">the rotation around the z axis</param>
function SetRotation(Id, rx, ry, rz) {
    var data = Primitives.get(Id);
    if (data != undefined) {
        var mesh = data["mesh"];
        mesh.rotation.x = rx;
        mesh.rotation.y = ry;
        mesh.rotation.z = rz;
    }
}

/// <summary>
/// Sets the position of the details panel.
/// </summary>
/// <param name="x">the translation along the x axis in screen coordinates</param>
/// <param name="y">the translation along the y axis in screen coordinates</param>
function SetPanelPosition(x, y) {
    DetailsPanel.style.left = x + 'px';
    DetailsPanel.style.top = y + 'px';
}

/// <summary>
/// Sets the visibility of the details panel.
/// </summary>
/// <param name="visible">the new visibility of the panel</param>
function SetPanelVisibility(visible) {
    if (visible) {
        DetailsPanel.style.display = 'block';
    } else {
        DetailsPanel.style.display = 'none';
    }
}

/// <summary>
/// Sets the content of the panel
/// </summary>
/// <param name="content">the content of the panel in HTML5 format</param>
function SetPanelContent(content) {
    DetailsPanel.innerHTML = content;
}

/// <summary>
/// Tries to get the world coordinates of the mouse cursor.
/// </summary>
/// <returns>An array of type [x,y,z] is the transformation can be done, null otherwise</returns>
function TryGetWorldCoordinates(x, y) {
    var hit = Scene.pick(x, y);
    if (hit.pickedMesh.CometID != 'Skybox') {
        var pt = hit.pickedPoint;
        return [pt._x, pt._y, pt._z];
    }
    return null;
}