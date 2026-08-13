// --------------------------------------------------------------------------------------------------------------------
// <copyright file="babylonInterop.js" company="Starion Group S.A.">
//    Copyright (c) 2023-2026 Starion Group S.A.
//
//    This file is part of CDP4-COMET WEB Community Edition
//    The CDP4-COMET WEB Community Edition is the Starion Web Application implementation of ECSS-E-TM-10-25 Annex A and Annex C.
//
//    The CDP4-COMET WEB Community Edition is free software; you can redistribute it and/or
//    modify it under the terms of the GNU Affero General Public
//    License as published by the Free Software Foundation; either
//    version 3 of the License, or (at your option) any later version.
//
//    The CDP4-COMET WEB Community Edition is distributed in the hope that it will be useful,
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
 * A map of viewer states indexed by viewerId.
 */
const ViewerStates = new Map();

/**
 * Gets or creates the viewer state for a given viewerId.
 * @param {string} viewerId - the viewer identifier.
 */
function GetViewerState(viewerId) {
    let state = ViewerStates.get(viewerId);

    if (!state) {
        state = {
            Scene: null,
            SceneObjects: new Map(),
            PickingMaterial: null,
            SceneSpecularColor: null,
            SceneEmissiveColor: null,
            SceneAmbientColor: null,
            BabylonCanvas: null,
            Camera: null,
            BabylonEngine: null,
            HighLightLayer: null
        };

        ViewerStates.set(viewerId, state);
    }
    return state;
}

/**
 * Inits the babylon.js scene on the canvas, the asociated resources and starts the render loop.
 * @param {string} viewerId - the unique identifier for the viewer instance.
 * @param {HTMLCanvasElement} canvas - the canvas the scene it's attached to.
 * @param {boolean} addAxes - whether to add world axes.
 */
function InitCanvas(viewerId, canvas, addAxes) {

    if (canvas == null) {
        throw "The canvas can't be null or undefined";
    }

    if (ViewerStates.has(viewerId)) {
        DisposeViewer(viewerId);
    }

    let state = GetViewerState(viewerId);
    state.BabylonCanvas = canvas;
    state.BabylonEngine = new BABYLON.Engine(state.BabylonCanvas, true, { stencil: true, antialias: true });

    if (state.BabylonEngine == null || state.BabylonEngine == undefined) {
        throw "The babylon engine cannot be initialized";
    }

    state.Scene = CreateScene(state.BabylonEngine, state.BabylonCanvas, state);

    if (state.Scene == null || state.Scene == undefined) {
        throw "The scene cannot be initialized";
    }

    state.HighLightLayer = new BABYLON.HighlightLayer("highlightLayer", state.Scene, {renderingGroupId:0});

    CreateSkybox(state.Scene, SkyboxSize);

    state.PickingMaterial = SetUpPickingMaterial(state.Scene);

    state.SceneSpecularColor = new BABYLON.Color3(1.0, 1.0, 1.0);
    state.SceneEmissiveColor = new BABYLON.Color3(0.0, 0.0, 0.0);
    state.SceneAmbientColor  = new BABYLON.Color3(1.0, 1.0, 1.0);

    state.BabylonEngine.runRenderLoop(function () {
        state.Scene.render();
    });

    state.ResizeListener = function () {
        if (state.BabylonEngine) {
            state.BabylonEngine.resize();
        }
    };
    window.addEventListener("resize", state.ResizeListener);

    if (addAxes)
    {
        AddWorldAxes(state);
    }
};

/**
 * Disposes the babylon engine and resources for a given viewer instance.
 * @param {string} viewerId - the viewer identifier.
 */
function DisposeViewer(viewerId) {
    let state = ViewerStates.get(viewerId);
    if (state) {
        if (state.ResizeListener) {
            window.removeEventListener("resize", state.ResizeListener);
        }
        if (state.SceneObjects) {
            for (let id of state.SceneObjects.keys()) {
                Dispose(state, id);
            }
        }
        if (state.Scene) {
            state.Scene.dispose();
        }
        if (state.BabylonEngine) {
            state.BabylonEngine.dispose();
        }
        ViewerStates.delete(viewerId);
    }
}

/*
 * Adds the world axes to the scene
 */
function AddWorldAxes(state) {
    let size = SkyboxSize / 2.0;
    //X Axis
    let lpoints = [
        new BABYLON.Vector3(-size, 0, 0),
        new BABYLON.Vector3(+size, 0, 0)
    ];
    let xaxis = BABYLON.MeshBuilder.CreateLines("lines", { points: lpoints }, state.Scene);
    xaxis.color = new BABYLON.Color3(255,0,0);

    //Y Axis
    lpoints = [
        new BABYLON.Vector3(0,-size,0),
        new BABYLON.Vector3(0,+size,0)
    ];
    let yaxis = BABYLON.MeshBuilder.CreateLines("lines", { points: lpoints }, state.Scene);
    yaxis.color = new BABYLON.Color3(0, 255, 0);

    //Z Axis
    lpoints = [
        new BABYLON.Vector3(0,0,-size),
        new BABYLON.Vector3(0,0,+size)
    ];
    let zaxis = BABYLON.MeshBuilder.CreateLines("lines", { points: lpoints }, state.Scene);
    zaxis.color = new BABYLON.Color3(0,0,255);
}

/**
 * Adds to scene an scene object containing the primitive
 * @param {string} viewerId - the viewer identifier.
 * @param {any} sceneObject - the scene object to add in JSON string format
 */
async function AddSceneObject(viewerId, sceneObject) {
    let state = GetViewerState(viewerId);
    sceneObject = JSON.parse(sceneObject);
    let primitive = sceneObject.Primitive;
    let mesh = null;

    if (primitive != null && primitive != undefined)
    {
        mesh = await AddPrimitive(primitive, state.Scene);
    }
    
    if (mesh != null) {
        FillMeshWithPrimitiveData(mesh, primitive, sceneObject.ID, state);
        let sceneObj = new SceneObject(sceneObject.ID, mesh, primitive);
        state.SceneObjects.set(sceneObject.ID, sceneObj);
    }
}

/**
 * Adds to scene an already parsed primitive. 
 * @param {any} primitive - already parsed
 * @param {BABYLON.Scene} scene - the babylon scene
 */
async function AddPrimitive(primitive, scene) {
    let mesh;

    switch (primitive.Type) {
        case "Line": mesh = CreateLine(primitive, scene); break;
        case "Cube": mesh = CreateBox(primitive, scene); break;
        case "Sphere": mesh = CreateSphere(primitive, scene); break;
        case "Cylinder": mesh = CreateCylinder(primitive, scene); break;
        case "Cone": mesh = CreateCone(primitive, scene); break;
        case "Torus": mesh = CreateTorus(primitive, scene); break;
        case "CustomPrimitive": await LoadPrimitive(primitive, scene); break;
        case "TriangularPrism": mesh = CreateTriangularPrism(primitive, scene); break;
        case "Disc": mesh = CreateDisc(primitive, scene); break;
        case "HexagonalPrism": mesh = CreateHexagonalPrism(primitive, scene); break;
        case "Rectangle": mesh = CreateRectangle(primitive, scene); break;
        case "Wedge": mesh = CreateWedge(primitive, scene); break;
        case "EquilateralTriangle": mesh = CreateTriangle(primitive, scene); break;

        default: throw `The type of the primitive [${primitive.Type}] is not defined in the JS file`;
    }

    return mesh;
}

/**
 * Fills the Babylon.JS mesh with the data of the C# Primitive
 * @param {any} mesh - the mesh to fill the data with
 * @param {any} primitive - the primitive used for filling the data
 * @param {string} ID - the object ID
 * @param {object} state - the viewer state
 */
function FillMeshWithPrimitiveData(mesh, primitive, ID, state) {

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

    let babylonMaterial = CreateMaterial(primitiveColor, state.SceneSpecularColor, state.SceneEmissiveColor, state.SceneAmbientColor, "DefaultMaterial", state.Scene);
    mesh.material = babylonMaterial;
    mesh.material.useLogarithmicDepth = true;
    mesh.renderingGroupId = primitive.RenderingGroup;

    mesh.actionManager = new BABYLON.ActionManager(state.Scene);
    RegisterMeshActions(mesh, state.PickingMaterial);

    //Custom properties for the object
    mesh.ObjectID = ID;
    mesh.Materials = new MeshMaterial(mesh.material, state.PickingMaterial);
    if (primitive.HasHalo) {
        state.HighLightLayer.addMesh(mesh, new BABYLON.Color3(1.0, 0.3, 0));
    }
}

/** 
 * Dispose all the scene objects with the specified ids 
 * @param {string} viewerId - the viewer identifier.
 * @param {any} IDs - a collection of ids  
 */
function DisposeAll(viewerId, IDs) {
    let state = GetViewerState(viewerId);
    if (IDs != null && IDs != undefined) {
        for (let i = 0; i < IDs.length; i++) {
            Dispose(state, IDs[i]);
        }
    }
} 

/**
 * Removes the primitive with the specified ID from the scene.
 * @param {object} state - the viewer state.
 * @param {number} ID - the ID of the primitive to delete.
 */
function Dispose(state, ID) {
    if (state.SceneObjects.size > 0)
    {
        let sceneObj = state.SceneObjects.get(ID);

        if (sceneObj != null && sceneObj != undefined)
        {
            let mesh = sceneObj.Mesh;
            mesh.dispose();
            mesh = null;
        }

        state.SceneObjects.delete(ID);
    }
}

/**
 * Get the ID of the primitive that is under the mouse cursor.
 * @param {string} viewerId - the viewer identifier.
 * @returns {string} the ID.
 */
function GetPrimitiveIDUnderMouse(viewerId) {
    let state = GetViewerState(viewerId);
    if (!state.Scene) {
        return null;
    }
    let hit = state.Scene.pick(state.Scene.pointerX, state.Scene.pointerY);
    if (hit && hit.pickedMesh) {
        let pickedMesh = hit.pickedMesh;

        if (pickedMesh.Name != "skyBox")
        {
            return pickedMesh.ObjectID;
        }
    }

    return null;
}

/**
 * Sets the visibility of the primitive
 * @param {string} viewerId - the viewer identifier.
 * @param {string} ID - the ID of the primitive to select
 * @param {boolean} isVisible - the value of the new selection
 */
function SetMeshVisibility(viewerId, ID, isVisible) {
    let state = GetViewerState(viewerId);
    if (state.SceneObjects.size > 0)
    {
        let sceneObj = state.SceneObjects.get(ID);
        if (sceneObj != null && sceneObj != undefined) {
            let mesh = sceneObj.Mesh;
            if (mesh != null && mesh != undefined) {
                mesh.setEnabled(isVisible);
            }
        } 
    }
}

/**
 * Regenerates the mesh asociated to the scene object
 * @param {string} viewerId - the viewer identifier.
 * @param {object} jsonSceneObject - the scene object to regenerate in JSON string format
 */
async function RegenMesh(viewerId, jsonSceneObject) {
    let state = GetViewerState(viewerId);
    let sceneFullObject = JSON.parse(jsonSceneObject);

    if (sceneFullObject != null)
    {
        Dispose(state, sceneFullObject.ID);
        await AddSceneObject(viewerId, jsonSceneObject);
    }
}
