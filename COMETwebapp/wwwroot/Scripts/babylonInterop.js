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

/**
 * Skybox size, used for the cube that surrounds the scene.
 * @type {number}
 */
const SkyboxSize = 1400.0;

/**
 * Camera rotation sensibility. A higher number means that the angle of rotation per mouse displacement is fewer.
 * @type {number}
 */
const CameraRotationSensibility = 900.0;

/**
 * Camera panning sensibility. A higher number means that the panning distance per mouse displacement is fewer.
 * @type {number}
 */
const CameraPanningSensibility = 80.0;

/**
 * Camera zoom sensibility. A higher number means that the zoom per mouse wheel displacement is fewer.
 * @type {number}
 */
const CameraZoomSensibility = 0.3;

/**
 * Camera inertia. Used to interpolate between animations. 
 * @type {number}
 */
const CameraInertia = 0.1;

/**
 * The babylon.js scene.
 * @type {BABYLON.JS Scene}
 */
let Scene;

/**
 * A list of the primitives that the scene contains.
 * @type {Map}
 */
let Primitives = new Map();

/**
 * A map of the SceneObjects that the Scene contains. 
 */
let SceneObjects = new Map();

/**
 * Picking material used when a primitive is hover with the mouse.
 * @type {BABYLON.js material}
 */
let PickingMaterial;

/**
 * The panel used for displaying primitive's details in the window.
 * @type {HTMLElement}
 */
let DetailsPanel;

/**
 * Scene specular color. 
 * @type {BABYLON.js Color}
 */
let SceneSpecularColor;

/**
 * Scene emissive color. 
 * @type {BABYLON.js Color}
 */
let SceneEmissiveColor;

/**
 * Scene ambient color. 
 * @type {BABYLON.js Color}
 */
let SceneAmbientColor;

/**
 * The HTML5 canvas where the scene is drawn.
 * @type {HTMLCanvasElement}
 */
let BabylonCanvas;

/**
 * The Babylon Camera
 * @type {Babylon.js Camera}
 */
let Camera;

/**
 * The Babylon Engine
 * @type {Babylon.js Engine}
 */
let BabylonEngine;

/**The layer used for highlightning */
let HighLightLayer;

/**
 * Inits the babylon.js scene on the canvas, the asociated resources and starts the render loop.
 * @param {HTMLCanvasElement} canvas - the canvas the scene it's attached to.
 */
function InitCanvas(canvas,addAxes) {

    if (canvas == null || canvas == undefined) {
        throw "The canvas can't be null or undefined";
    }

    BabylonCanvas = canvas;
    BabylonEngine = new BABYLON.Engine(BabylonCanvas, true, { stencil: true, antialias: true });

    if (BabylonEngine == null || BabylonEngine == undefined) {
        throw "The babylon engine cannot be initialized";
    }

    Scene = CreateScene(BabylonEngine, BabylonCanvas);

    if (Scene == null || Scene == undefined) {
        throw "The scene cannot be initialized";
    }

    HighLightLayer = new BABYLON.HighlightLayer("highlightLayer", Scene, {renderingGroupId:0});

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

    if (addAxes)
    {
        AddWorldAxes();
    }
};

function AddWorldAxes() {
    let size = SkyboxSize / 2.0;
    //X Axis
    let lpoints = [
        new BABYLON.Vector3(-size, 0, 0),
        new BABYLON.Vector3(+size, 0, 0)
    ];
    let xaxis = BABYLON.MeshBuilder.CreateLines("lines", { points: lpoints }, Scene);
    xaxis.color = new BABYLON.Color3(255,0,0);

    //Y Axis
    lpoints = [
        new BABYLON.Vector3(0,-size,0),
        new BABYLON.Vector3(0,+size,0)
    ];
    let yaxis = BABYLON.MeshBuilder.CreateLines("lines", { points: lpoints }, Scene);
    yaxis.color = new BABYLON.Color3(0, 255, 0);

    //Z Axis
    lpoints = [
        new BABYLON.Vector3(0,0,-size),
        new BABYLON.Vector3(0,0,+size)
    ];
    let zaxis = BABYLON.MeshBuilder.CreateLines("lines", { points: lpoints }, Scene);
    zaxis.color = new BABYLON.Color3(0,0,255);
}

/**
 * Gets the scene size
 * @returns the size in the format [width,height]
 */
function GetCanvasSize() {
    return [BabylonCanvas.width, BabylonCanvas.height];
}

/**
 * Adds to scene an scene object containing the primitive
 * @param {any} sceneObject
 */
function AddSceneObject(sceneObject) {
    sceneObject = JSON.parse(sceneObject);
    let primitive = sceneObject.Primitive;
    AddPrimitive(primitive);
}

/**
 * Adds to scene an already parsed primitive. 
 * @param {any} primitive - already parsed
 */
function AddPrimitive(primitive) {
    let mesh;

    switch (primitive.Type) {
        case "Line": mesh = CreateLine(primitive); break;
        case "Cube": mesh = CreateBox(primitive); break;
        case "Sphere": mesh = CreateSphere(primitive); break;
        case "Cylinder": mesh = CreateCylinder(primitive); break;
        case "Cone": mesh = CreateCone(primitive); break;
        case "Torus": mesh = CreateTorus(primitive); break;
        case "CustomPrimitive": LoadPrimitive(primitive); break;
        case "TriangularPrism": mesh = CreateTriangularPrism(primitive); break;
        case "Disc": mesh = CreateDisc(primitive); break;
        case "HexagonalPrism": mesh = CreateHexagonalPrism(primitive); break;
        case "Rectangle": mesh = CreateRectangle(primitive); break;
        case "Wedge": mesh = CreateWedge(primitive); break;
        case "EquilateralTriangle": mesh = CreateTriangle(primitive); break;

        default: throw `The type of the primitive [${primitive.Type}] is not defined in the JS file`;
    }

    if (mesh != null) {
        FillMeshWithPrimitiveData(mesh, primitive);
        let sceneObj = new SceneObject(mesh, primitive);
        SceneObjects.set(mesh.ObjectID, sceneObj);
    }
}

/**
 * Fills the Babylon.JS mesh with the data of the C# Primitive
 * @param {any} mesh - the mesh to fill the data with
 * @param {any} primitive - the primitive used for filling the data
 */
function FillMeshWithPrimitiveData(mesh, primitive) {

    mesh.position.x = primitive.X;
    mesh.position.y = primitive.Y;
    mesh.position.z = primitive.Z;

    mesh.rotation.x = primitive.RX;
    mesh.rotation.y = primitive.RY;
    mesh.rotation.z = primitive.RZ;

    let primitiveColor =
    {
        X: primitive.Color.X / 255.0,
        Y: primitive.Color.Y / 255.0,
        Z: primitive.Color.Z / 255.0,
    }

    let babylonMaterial = CreateMaterial(primitiveColor, SceneSpecularColor, SceneEmissiveColor, SceneAmbientColor, "DefaultMaterial", Scene);
    mesh.material = babylonMaterial;
    mesh.material.useLogarithmicDepth = true;
    mesh.renderingGroupId = primitive.RenderingGroup;

    mesh.actionManager = new BABYLON.ActionManager(Scene);
    RegisterMeshActions(mesh);

    //Custom properties for the object
    mesh.ObjectID = primitive.ID;
    mesh.Materials = new MeshMaterial(mesh.material, PickingMaterial);
}

/**
 * Removes the primitive with the specified ID from the scene.
 * @param {number} ID - the ID of the primitive to delete.
 */
function Dispose(ID) {
    if (SceneObjects.size > 0)
    {
        let sceneObj = SceneObjects.get(ID);
        let mesh = sceneObj.Mesh;
        mesh.dispose();
        mesh = null;
        SceneObjects.delete(ID);
    }
}

/**
 * Get the ID of the primitive that is under the mouse cursor.
 * @returns {string} the ID.
 */
function GetPrimitiveIDUnderMouse() {
    let hit = Scene.pick(Scene.pointerX, Scene.pointerY)
    let pickedMesh = hit.pickedMesh;

    if (pickedMesh.Name != "skyBox")
    {
        return pickedMesh.ObjectID;
    }

    return null;
}

/**
 * Sets the translation of the primitive with the specified ID
 * @param {string} ID - the ID of the primitive to translate.
 * @param {number} x - translation along the X axis
 * @param {number} y - translation along the Y axis
 * @param {number} z - translation along the Z axis
 */
function SetPrimitivePosition(ID, x, y, z) {

    if (SceneObjects.size > 0) {
        let sceneObj = SceneObjects.get(ID);
        let mesh = sceneObj.Mesh;
        mesh.position.x = x;
        mesh.position.y = y;
        mesh.position.z = z;
    }
}

/**
 * Sets the rotation of the primitive with the specified ID
 * @param {string} ID - the ID of the primitive to rotate.
 * @param {number} rx - the rotation around X axis
 * @param {number} ry - the rotation around Y axis
 * @param {number} rz - the rotation around Z axis
 */
function SetPrimitiveRotation(ID, rx, ry, rz) {

    if (SceneObjects.size > 0) {
        let sceneObj = SceneObjects.get(ID);
        let mesh = sceneObj.Mesh;
        mesh.rotation.x = rx;
        mesh.rotation.y = ry;
        mesh.rotation.z = rz;
    }
}

/**
 * Sets the selection of the primitive
 * @param {string} ID - the ID of the primitive to select
 * @param {boolean} isSelected - the value of the new selection
 */
function SetSelection(ID, isSelected) {

    if (SceneObjects.size > 0)
    {
        let sceneObject = SceneObjects.get(ID);
        let mesh = sceneObject.Mesh;
        if (isSelected)
        {
            HighLightLayer.addMesh(mesh, new BABYLON.Color3(1.0, 0.3, 0));
        }
        else
        {
            HighLightLayer.removeMesh(mesh);
        }
    }
}

/**
 * Sets the visibility of the primitive
 * @param {string} ID - the ID of the primitive to select
 * @param {boolean} isVisible - the value of the new selection
 */
function SetMeshVisibility(ID, isVisible) {

    if (SceneObjects.size > 0)
    {
        let sceneObj = SceneObjects.get(ID);
        let mesh = sceneObj.Mesh;
        mesh.setEnabled(isVisible);
    }
}

/**
 * Regenerates the mesh asociated to the primitive
 * @param {object} Primitive - the pritimive to regenerate in JSON string format
 */
function RegenMesh(Primitive) {
    let primitive = JSON.parse(Primitive);
    let sceneObject = SceneObjects.get(primitive.ID);

    if (sceneObject != null)
    {
        Dispose(primitive.ID);
        AddPrimitive(primitive);
    }
}

/**
 * Sets the mesh color
 * @param {any} ID - the ID of the primitive to change the color
 * @param {any} r - the red component in range [0,255]
 * @param {any} g - the green component in range [0,255]
 * @param {any} b - the blue component in range [0,255]
 */
function SetMeshColor(ID, r, g, b) {

    if (SceneObjects.size > 0) {
        let sceneObj = SceneObjects.get(ID);
        let mesh = sceneObj.Mesh;
        mesh.material.diffuseColor = new BABYLON.Color3(r / 255.0, g / 255.0, b / 255.0);
    }
}