/**
 * Sets the dotnet helper
 * @param {any} helper
 */
function setDotNetHelper(helper) {
    dotNetHelper = helper;
}

/**
 * Gets an element size and position relative to its parent
 * @param {int} index
 * @param {string} cssSelector
 * @param {bool} useScroll
 */
function GetElementSizeAndPosition(index, cssSelector, useScroll)
{
    let elements = document.getElementsByClassName(cssSelector);
    let element = elements[index];
    
    if (element != null)
    {
        try
        {
            if (useScroll)
            {
                let offsetTop = element.getBoundingClientRect().top - element.offsetParent.getBoundingClientRect().top;
                return [element.offsetLeft, offsetTop, element.offsetWidth, element.offsetHeight];
            }

            return [element.offsetLeft, element.offsetTop, element.offsetWidth, element.offsetHeight];
        }
        catch (error)
        {
            return [0, 0, 0, 0];
        }
    }
    
    return [0, 0, 0, 0];
}

/**
 * Subscribes to the resize event of the window and use a callback method to alert
 * @param {string} callbackMethodName
 */
function SubscribeToResizeEvent(callbackMethodName)
{
    window.addEventListener("resize", () =>
    {
        try
        {
            dotNetHelper.invokeMethodAsync(callbackMethodName);
        }
        catch (error) {
            console.log(error);
        }
    });
}