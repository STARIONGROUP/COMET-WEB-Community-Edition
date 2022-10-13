"use strict";

//---------------------------
//------- CONSTANTS ---------
//---------------------------
const skyboxSize = 1400.0;
const cameraSensibility = 900.0;
const cameraPanningSensibility = 100;
const cameraZoomSensibility = 0.3;
const cameraInertia = 0.1;

//---------------------------
//------- Variables ---------
//---------------------------
var babylonCanvas;
var babylonEngine;
var scene;
var Primitives = new Map();
var highlightlayer;
var pickingMaterial;
var detailsPanel;
var sceneSpecularColor;
var sceneEmissiveColor;
var sceneAmbientColor;

function InitCanvas() {
    babylonCanvas = document.getElementById("babylon-canvas");
    babylonEngine = new BABYLON.Engine(babylonCanvas, true, { stencil: true, antialias: true });
    scene = CreateScene(babylonEngine, babylonCanvas);
    CreateSkybox(scene, skyboxSize);

    pickingMaterial = SetUpPickingMaterial();
    detailsPanel = document.getElementById("detailsPanel");

    sceneSpecularColor = new BABYLON.Color3(1.0, 1.0, 1.0);
    sceneEmissiveColor = new BABYLON.Color3(0.0, 0.0, 0.0);
    sceneAmbientColor  = new BABYLON.Color3(1.0, 1.0, 1.0);

    babylonEngine.runRenderLoop(function () {
        scene.render();
    });

    window.addEventListener("resize", function () {
        babylonEngine.resize();
    });
};

function CreateScene (engine, canvas) {
    var scene = new BABYLON.Scene(engine);
    scene.clearColor = new BABYLON.Color3(0.98, 0.98, 0.98);

    var camera = new BABYLON.ArcRotateCamera("Camera", 0, 0, 10, new BABYLON.Vector3(0, 0, 0), scene);
    camera.setPosition(new BABYLON.Vector3(200, 100, -300));
    camera.attachControl(canvas, true);
    camera.lowerRadiusLimit = 5;
    camera.upperRadiusLimit = skyboxSize / 2.0;
    camera.inertia = cameraInertia;
    camera.panningInertia = cameraInertia;
    camera.angularSensibilityX = camera.angularSensibilityY = cameraSensibility;
    camera.panningSensibility = cameraPanningSensibility;
    camera.wheelPrecision = cameraZoomSensibility;
   
    var light1 = new BABYLON.HemisphericLight("HemisphericLight", new BABYLON.Vector3(2, 1, 0), scene);
    
    return scene;
};

function GetPrimitiveUnderMouse() {
    var hit = scene.pick(scene.pointerX, scene.pointerY)
    var pickedMesh = hit.pickedMesh;

    if (pickedMesh.CometID != "Skybox")
    {
        return pickedMesh.CometID;
    }

    return null;
}


function RegisterMeshActions(mesh) {
    mesh.actionManager.registerAction(new BABYLON.SetValueAction(BABYLON.ActionManager.OnPointerOutTrigger,  mesh.material, "emissiveColor", mesh.material.emissiveColor));
    mesh.actionManager.registerAction(new BABYLON.SetValueAction(BABYLON.ActionManager.OnPointerOverTrigger, mesh.material, "emissiveColor", pickingMaterial.emissiveColor));
    mesh.actionManager.registerAction(new BABYLON.SetValueAction(BABYLON.ActionManager.OnPointerOutTrigger,  mesh.material, "diffuseColor",  mesh.material.diffuseColor));
    mesh.actionManager.registerAction(new BABYLON.SetValueAction(BABYLON.ActionManager.OnPointerOverTrigger, mesh.material, "diffuseColor",  pickingMaterial.diffuseColor));
    mesh.actionManager.registerAction(new BABYLON.SetValueAction(BABYLON.ActionManager.OnPointerOutTrigger,  mesh.material, "specularColor", mesh.material.specularColor));
    mesh.actionManager.registerAction(new BABYLON.SetValueAction(BABYLON.ActionManager.OnPointerOverTrigger, mesh.material, "specularColor", pickingMaterial.specularColor));
    mesh.actionManager.registerAction(new BABYLON.SetValueAction(BABYLON.ActionManager.OnPointerOutTrigger,  mesh.material, "ambientColor",  mesh.material.ambientColor));
    mesh.actionManager.registerAction(new BABYLON.SetValueAction(BABYLON.ActionManager.OnPointerOverTrigger, mesh.material, "ambientColor",  pickingMaterial.ambientColor));
}


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

//--------------------------------------
//---------- CREATE ENTITIES -----------
//--------------------------------------
function CreateLine(primitive, color) {
    const lpoints = [
        new BABYLON.Vector3(primitive.P0.X, primitive.P0.Y, primitive.P0.Z),
        new BABYLON.Vector3(primitive.P1.X, primitive.P1.Y, primitive.P1.Z)
    ];
    var line = BABYLON.MeshBuilder.CreateLines("lines", { points: lpoints }, scene);
    line.color = new BABYLON.Color3(color.X, color.Y, color.Z);
    return line;
}

function CreateBox(primitive, color) {
    var mesh = BABYLON.MeshBuilder.CreateBox("box", { width: primitive.Width, height: primitive.Height, depth: primitive.Depth }, scene);
    AssignInitialMaterialAndTransformation(mesh, primitive, color);
    return mesh;
}

function CreateSphere(primitive, color) {
    var mesh = BABYLON.MeshBuilder.CreateSphere("sphere", { diameter: primitive.Radius * 2.0 }, scene);
    AssignInitialMaterialAndTransformation(mesh, primitive, color);
    return mesh;
}

function CreateCylinder(primitive, color) {
    var mesh = BABYLON.MeshBuilder.CreateCylinder("cone", { diameter: primitive.Radius * 2.0, height: primitive.Height }, scene);
    AssignInitialMaterialAndTransformation(mesh, primitive, color);
    return mesh;
}

function CreateCone(primitive, color) {
    var mesh = BABYLON.MeshBuilder.CreateCylinder("cone", { diameterTop: 0, diameterBottom: primitive.Radius * 2.0, height: primitive.Height }, scene);
    AssignInitialMaterialAndTransformation(mesh, primitive, color);
    return mesh;
}

function CreateTorus(primitive, color) {
    var mesh = BABYLON.MeshBuilder.CreateTorus("torus", { diameter: primitive.Diameter, thickness: primitive.Thickness, tessellation : 36 }, scene);
    AssignInitialMaterialAndTransformation(mesh, primitive, color);
    return mesh;
}

async function LoadPrimitive(primitive, color) {
    var path = primitive.Path;
    var fileName = primitive.FileName;

    const result = await BABYLON.SceneLoader.ImportMeshAsync(null, path, fileName, scene);
    var meshes = result.meshes;

    for (let i = 0; i < meshes.length; i++) {
        AssignInitialMaterialAndTransformation(meshes[i], primitive, color);
    }
}

function AssignInitialMaterialAndTransformation(mesh, primitive, color) {

    if (primitive.hasOwnProperty("Subtype") && primitive.Subtype == "Positionable") {
        mesh.position.x = primitive.X;
        mesh.position.y = primitive.Y;
        mesh.position.z = primitive.Z;

        mesh.rotation.x = primitive.RX;
        mesh.rotation.y = primitive.RY;
        mesh.rotation.z = primitive.RZ;
    }

    var babylonMaterial = CreateMaterial(color, sceneSpecularColor, sceneEmissiveColor, sceneAmbientColor, "DefaultMaterial", scene);
    mesh.material = babylonMaterial;

    mesh.actionManager = new BABYLON.ActionManager(scene);
    RegisterMeshActions(mesh);

    //Custom properties for the object
    mesh.CometID = primitive.ID;
    mesh.Materials = [mesh.material, pickingMaterial];

    Primitives.set(primitive.ID, { "mesh": mesh, "primitive": primitive });
}

//------------------------
//---REMOVE PRIMITIVES----
//------------------------

function Dispose(id) {
    var data = Primitives.get(id);
    var mesh = data["mesh"];
    if (mesh != null) {
        mesh.dispose();
    }
}


//------------------------
//-----TRANSFORMATIONS----
//------------------------

function SetTranslation(Id, x, y, z) {    
    var data = Primitives.get(Id);
    if (data != undefined) {
        var mesh = data["mesh"];
        mesh.position.x = x;
        mesh.position.y = y;
        mesh.position.z = z;
    }
}

function SetRotation(Id, rx, ry, rz) {
    var data = Primitives.get(Id);
    if (data != undefined) {
        var mesh = data["mesh"];
        mesh.rotation.x = rx;
        mesh.rotation.y = ry;
        mesh.rotation.z = rz;
    }
}

function GetPrimitiveInfo(Id) {
    var data = Primitives.get(Id);
    var primitiveData = data["primitive"];

    var info =
        "The actual info is static data from when the primitive was added to the scene." + "<br>" +
        "ID: " + primitiveData.ID + "<br>" +
        "Type: " + primitiveData.Type + "<br>" +
        "X: " + primitiveData.X + "<br>" +
        "Y: " + primitiveData.Y + "<br>" +
        "Z: " + primitiveData.Z + "<br>" +
        "RX: " + primitiveData.RX + "<br>" +
        "RY: " + primitiveData.RY + "<br>" +
        "RZ: " + primitiveData.RZ;

    return info;
}

//---------------------------
//----- DETAILS PANEL -------
//---------------------------
function SetPanelPosition(x, y) {
    detailsPanel.style.left = x + 'px';
    detailsPanel.style.top = y + 'px';
}

function SetPanelVisibility(visible) {
    if (visible) {
        detailsPanel.style.display = 'block';
    } else {
        detailsPanel.style.display = 'none';
    }
}

function SetPanelContent(content) {
    detailsPanel.innerHTML = content;
}


function TryGetWorldCoordinates(x, y) {
    var hit = scene.pick(x, y);
    if (hit.pickedMesh.CometID != 'Skybox') {
        var pt = hit.pickedPoint;
        console.log(hit.pickedPoint);
        console.log(pt);
        console.log([pt._x, pt._y, pt._z]);
        return [pt._x, pt._y, pt._z];
    }
    return null;
}