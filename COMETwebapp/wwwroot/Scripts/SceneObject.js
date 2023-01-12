/**
 * Class of Scene Object used in Babylon.js
 */
class SceneObject {
    constructor(sceneObjectID, mesh, primitive) {
        this.sceneObjectID = sceneObjectID;
        this.Mesh = mesh;
        this.Primitive = primitive;
    }
}