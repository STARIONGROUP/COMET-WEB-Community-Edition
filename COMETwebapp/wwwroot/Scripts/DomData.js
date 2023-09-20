function GetElementSizeAndPosition(index, cssSelector, useScroll)
{
    var elements = document.getElementsByClassName(cssSelector);
    var element = elements[index];
    
    if (element != null)
    {
        try
        {
            if (useScroll)
            {
                var offsetTop = element.getBoundingClientRect().top - element.offsetParent.getBoundingClientRect().top;
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

function SubscribeToResizeEvent(callbackMethodName) {
    window.addEventListener("resize", () =>
    {
        DotNet.invokeMethodAsync('COMETwebapp', callbackMethodName);
    });
}