var selected = null;
function setVisibility(elem)
{
    if (elem.style.visibility == 'hidden')
    {
        elem.style.visibility = 'visible';
    }
    else
    {
        elem.style.visibility = 'hidden';
    }
}

function setColor(elem, MouseOverCssClass)
{
    if(elem == selected || elem == null)
    return;
    elem.className = MouseOverCssClass;
    elem.style.cursor='default';
}

function setColorOut(elem, MouseOutCssClass)
{
    if(elem == selected || elem == null)
        return;
    elem.className = MouseOutCssClass;
    elem.style.cursor='default';
}

function select(elem, num, selectedIndex, CssClass, SelectedItemCssClass)
{
    if(elem == selected || elem == null)
        return;
    else if(selected != null)
    {
        selected.className = CssClass;
    }
    setVisibility(elem);
    selected = elem;
    document.getElementById(selectedIndex).value = num;
}