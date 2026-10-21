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
            script.onerror = () => {
                script.remove();
                babylonScriptsPromise = null;
                reject(new Error(`Failed to load script: ${src}`));
            };
            document.body.appendChild(script);
        }));
    }, Promise.resolve());

    return babylonScriptsPromise;
}

/**
 * Stops the browser from navigating to a file that the user drops outside a drop zone. Without this a near miss on the
 * Annex C3 archive drop zone makes the browser open the archive instead of uploading it, which looks like the upload
 * silently failed. Drops that land on a file input still reach it, so the drop zone keeps working.
 */
(function preventStrayFileDrops() {
    const isFileDrop = event => Array.from(event.dataTransfer?.types ?? []).includes('Files');

    window.addEventListener('dragover', event => {
        if (isFileDrop(event) && event.target?.type !== 'file') {
            event.preventDefault();
            event.dataTransfer.dropEffect = 'none';
        }
    });

    window.addEventListener('drop', event => {
        if (isFileDrop(event) && event.target?.type !== 'file') {
            event.preventDefault();
        }
    });
})();
