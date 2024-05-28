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