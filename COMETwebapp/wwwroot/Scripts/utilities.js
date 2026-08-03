/**
 * Downloads a given file by its name
 * @param {string} fileName
 * @param {any} contentStreamReference
 */
async function DownloadFileFromStream(fileName, contentStreamReference) {
    const arrayBuffer = await contentStreamReference.arrayBuffer();
    const blob = new Blob([arrayBuffer], { type: 'text/plain;charset=utf-8' });

    const url = URL.createObjectURL(blob);

    const anchorElement = document.createElement('a');

    anchorElement.href = url;
    anchorElement.download = fileName ?? '';
    anchorElement.target = "_blank";
    anchorElement.click();
    anchorElement.remove();
    URL.revokeObjectURL(url);
}

/**
 * Gets an item dimensions
 * @param {any} cssSelector
 * @returns An array containing two integer elements if the element is found, otherwise an empty array
 */
function GetItemDimensions(cssSelector) {
    let element = document.querySelector(cssSelector);
    if (element != null) {
        return [element.clientWidth, element.clientHeight];
    }
    return [];
}

let babylonScriptsPromise = null;

/**
 * Dynamically loads Babylon.js scripts on demand
 * @returns {Promise<void>}
 */
function loadBabylonScripts() {
    if (babylonScriptsPromise) {
        return babylonScriptsPromise;
    }

    const scripts = [
        "Scripts/BabylonJS/babylon.js",
        "Scripts/BabylonJS/babylonjs.loaders.min.js",
        "Scripts/SceneObject.js",
        "Scripts/MeshMaterial.js",
        "Scripts/babylonSpecifics.js",
        "Scripts/babylonInterop.js"
    ];

    babylonScriptsPromise = scripts.reduce((promise, src) => {
        return promise.then(() => new Promise((resolve, reject) => {
            if (document.querySelector(`script[src="${src}"]`)) {
                resolve();
                return;
            }
            const script = document.createElement("script");
            script.src = src;
            script.onload = resolve;
            script.onerror = (err) => {
                babylonScriptsPromise = null;
                reject(err);
            };
            document.body.appendChild(script);
        }));
    }, Promise.resolve());

    return babylonScriptsPromise;
}