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
const CameraPanningSensibility = 100;

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
var Scene;

/**
 * A list of the primitives that the scene contains.
 * @type {Map}
 */
var Primitives = new Map();

/**
 * Picking material used when a primitive is hover with the mouse.
 * @type {BABYLON.js material}
 */
var PickingMaterial;

/**
 * The panel used for displaying primitive's details in the window.
 * @type {HTMLElement}
 */
var DetailsPanel;

/**
 * Scene specular color. 
 * @type {BABYLON.js Color}
 */
var SceneSpecularColor;

/**
 * Scene emissive color. 
 * @type {BABYLON.js Color}
 */
var SceneEmissiveColor;

/**
 * Scene ambient color. 
 * @type {BABYLON.js Color}
 */
var SceneAmbientColor;

/**
 * The HTML5 canvas where the scene is drawn.
 * @type {HTMLCanvasElement}
 */
var BabylonCanvas;

/**
 * The Babylon Camera
 * @type {Babylon.js Camera}
 */
var Camera;

/**
 * The Babylon Engine
 * @type {Babylon.js Engine}
 */
var BabylonEngine;

/**
 * Inits the babylon.js scene on the canvas, the asociated resources and starts the render loop.
 * @param {HTMLCanvasElement} canvas - the canvas the scene it's attached to.
 */
function InitCanvas(canvas) {

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

/**
 * Gets the scene size
 * @returns the size in the format [width,height]
 */
function GetCanvasSize() {
    return [BabylonCanvas.width, BabylonCanvas.height];
}

/**
 * Adds the primitive with the specified color to the scene.
 * @param {any} primitive - a JSON string representation of the primitive.
 * @param {any} color - a JSON string representation of the color.
 */
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

/**
 * Removes the primitive with the specified ID from the scene.
 * @param {number} ID - the ID of the primitive to delete.
 */
function Dispose(ID) {
    if (Primitives.size > 0) {
        let data = Primitives.get(ID);
        let mesh = data["mesh"];
        if (mesh != null) {
            mesh.dispose();
        }
    }
}

/**
 * Get the ID of the primitive that is under the mouse cursor.
 * @returns {string} the ID.
 */
function GetPrimitiveIDUnderMouse() {
    let hit = Scene.pick(Scene.pointerX, Scene.pointerY)
    let pickedMesh = hit.pickedMesh;

    if (pickedMesh.CometID != "Skybox") {
        return pickedMesh.CometID;
    }

    return null;
}

/**
 * Get the primitive by the ID
 * @param {any} Id - the Id of the primitive to retrieve
 * @returns {any} - the primitive if the ID is valid, null otherwise
 */
function GetPrimitiveByID(Id) {
    if (Primitives.size > 0) {
        let data = Primitives.get(Id);
        if (data != undefined) {
            let primitiveData = data["primitive"];
            let primitiveString = JSON.stringify(data["primitive"]);
            return [primitiveData.Type, primitiveString];
        }
    }
    return null;
}

/**
 * Sets the translation of the primitive with the specified ID
 * @param {number} ID - the ID of the primitive to translate.
 * @param {number} x - translation along the X axis
 * @param {number} y - translation along the Y axis
 * @param {number} z - translation along the Z axis
 */
function SetPrimitivePosition(ID, x, y, z) {
    if (Primitives.size > 0) {
        let data = Primitives.get(ID);
        if (data != undefined) {
            let mesh = data["mesh"];
            mesh.position.x = x;
            mesh.position.y = y;
            mesh.position.z = z;
        }
    }
}

/**
 * Sets the rotation of the primitive with the specified ID
 * @param {number} Id - the ID of the primitive to rotate.
 * @param {number} rx - the rotation around X axis
 * @param {number} ry - the rotation around Y axis
 * @param {number} rz - the rotation around Z axis
 */
function SetPrimitiveRotation(Id, rx, ry, rz) {
    if (Primitives.size > 0) {
        let data = Primitives.get(Id);
        if (data != undefined) {
            let mesh = data["mesh"];
            mesh.rotation.x = rx;
            mesh.rotation.y = ry;
            mesh.rotation.z = rz;
        }
    }
}

/**
 * Sets the position of the details panel.
 * @param {number} x - translation along X axis in screen coordinates (px)
 * @param {number} y - translation along Y axis in screen coordinates (px)
 */
function SetPanelPosition(x, y) {
    DetailsPanel.style.left = x + 'px';
    DetailsPanel.style.top = y + 'px';
}

/**
 * Sets the visibility of the details panel.
 * @param {boolean} visible - the new visibility of the panel.
 */
function SetPanelVisibility(visible) {
    if (visible) {
        DetailsPanel.style.display = 'block';
    } else {
        DetailsPanel.style.display = 'none';
    }
}

/**
 * Sets the content of the panel.
 * @param {HTMLElement} content - the content of the panel in HTML5 format.
 */
function SetPanelContent(content) {
    DetailsPanel.innerHTML = content;
}

/**
 * Tries to get the world coordinates of the mouse cursor.
 * @param {number} x
 * @param {number} y
 * @returns {Array} - An array of type [x,y,z] if the transformation is done, null otherwi
 */
function GetWorldCoordinates(x, y) {
    let hit = Scene.pick(x, y);
    if (hit.pickedMesh.CometID != 'Skybox') {
        let pt = hit.pickedPoint;
        return [pt._x, pt._y, pt._z];
    }
    return null;
}

/**
 * Get the screen coordinates by projecting the specified world coordinates
 * @param {number} x - the world X coordinate
 * @param {number} y - the world Y coordinate
 * @param {number} z - the world Z coordinate
 * @returns {Array} - Array of type [x,y] with the screen coordinates
 */
function GetScreenCoordinates(x,y,z) {
    let coordinates = BABYLON.Vector3.Project(new BABYLON.Vector3(x,y,z),
        BABYLON.Matrix.Identity(),
        Scene.getTransformMatrix(),
        Camera.viewport.toGlobal(
            BabylonEngine.getRenderWidth(),
            BabylonEngine.getRenderHeight(),
        ));
    return [coordinates.x, coordinates.y];
}