function GetElementSizeAndPosition(index, cssSelector, useScroll) {

    var elements = document.getElementsByClassName(cssSelector);
    var element = elements[index];
    
    if (element != null)
    {
        try
        {
            if (useScroll)
            {
                console.log("Position using scroll...")
                var offsetTop = element.getBoundingClientRect().top - element.offsetParent.getBoundingClientRect().top;
                return [element.offsetLeft, offsetTop, element.offsetWidth, element.offsetHeight];
            }

            console.log("Position NOT using scroll...")
            return [element.offsetLeft, element.offsetTop, element.offsetWidth, element.offsetHeight];
        }
        catch (error)
        {
            console.log(error);
            return [0, 0, 0, 0];
        }
    }
    
    return [0, 0, 0, 0];
}
