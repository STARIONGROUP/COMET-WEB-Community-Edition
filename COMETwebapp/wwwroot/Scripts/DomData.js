function GetElementSizeAndPosition(index, cssSelector) {

    var elements = document.getElementsByClassName(cssSelector);
    var element = elements[index];
    console.log(elements);
    if (element != null) {
        try {
            console.log(element);
            return [element.offsetLeft, element.offsetTop, element.offsetWidth, element.offsetHeight];
        } catch (error) {
            return [0, 0, 0, 0];
        }
    }
    console.log("Element not found");
    return [0, 0, 0, 0];
}