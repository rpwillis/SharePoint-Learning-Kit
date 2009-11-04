/* MICROSOFT PROVIDES SAMPLE CODE "AS IS" AND WITH ALL FAULTS, 
AND WITHOUT ANY WARRANTY WHATSOEVER.  MICROSOFT EXPRESSLY DISCLAIMS ALL WARRANTIES 
WITH RESPECT TO THE SOURCE CODE, INCLUDING BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF 
MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE.  THERE IS NO WARRANTY OF TITLE OR 
NONINFRINGEMENT FOR THE SOURCE CODE. */

/* Copyright (c) Microsoft Corporation. All rights reserved. */

// TOC.js

// Functions used in the table of contents of the frameset. This code is shared between SLK and BWP.
function FindActivityId( element )
{ 
    try
    {
        if (element != undefined)
         {
            while ((element.attributes["activityId"] == undefined) && (element.parentNode != null))
            {
                element = element.parentNode;
            }
           
            return element.attributes["activityId"].nodeValue;
         }
        else
        {
            return null;
        }
    }
    catch(err)
    {
  	    return null;
    }
}

// Find the <a> element that is visible. It's either the one with the activityId = strActivityId, or,
// if that node is not visible, a parent of that node.
function FindVisibleActivity( strActivityId )
{
    if (strActivityId == null)
        return null;
        
    // visible activity is always an <a> element
    var aId = "a" + strActivityId;
    var element = document.getElementById(aId);
    if (element == undefined)   
    {
        // there's no 'a', so look for a div (indicating the leaf is invisible)
        element = document.getElementById("div" + strActivityId);
    }
    else
    {
        // return the activity if it's visible
        if (element.style.visibility == "visible")
        {
            //alert("inside the else returning element with value of " + element);
            return element;
        }
    }
    
    // search for a child in the parent hierarchy that is visible
    //while (element.parentElement != null)
    while (element.parentNode != null)
    {        
        // go up the hierarchy until we find the next <a> element
        //var parent = element.parentElement;
        var parent = element.parentNode;
        
        for (i in parent.childNodes)
        {
            var child = parent.childNodes[i];
            if ((child.tagName == "A") && (child.id != aId))
            {
                if (child.style.visibility == "visible")
                {
                    return child;
                }
                else
                    break;  // we found a match, but it's not visible, so go up the tree again
            }
        }
        element =  parent;
    }
    return element;
}

// Make the current activity node visible in the table of contents by expanding its 
// cluster, and that of all parents.
function ExpandActivity( strActivityId )
{
    if (strActivityId == undefined)
        return;
        
    var elCluster = document.getElementById("divCluster" + strActivityId);
    
    if (elCluster != undefined)
    {
        ExpandCollapseGroup(strActivityId, elCluster, true);
    }
    
    // Check parent hierarchy. We first find the div containing the current activity, then 
    // find the next parent in the chain that has an activity id. 
    var divCurrentActivity = document.getElementById("div" + strActivityId);
    //var parentActivityId = FindActivityId ( divCurrentActivity.parentElement );
    var parentActivityId = FindActivityId ( divCurrentActivity.parentNode );
    
    ExpandActivity( parentActivityId );        
}

// Move the selection of the current element from previous one to new one.
// strActivitIdNew is the activity id of the newly chose current element.
// Returns true if the current element has changed.
function SetCurrentElement( strActivityIdNew )
{
    // If the new activity id is -1, it's just initializing.
    if (strActivityIdNew == -1)
        return false;
    
    var elNewActivity = FindVisibleActivity( strActivityIdNew );
    elNewActivity.style.fontWeight = "bold";
    elNewActivity.focus();
    
    var elOldActivity = FindVisibleActivity( g_currentActivityId );
    if ((elOldActivity != null)
        && (elOldActivity.id != elNewActivity.id))  // in case user is moving between invisible activities of the same parent
    {
       elOldActivity.style.fontWeight = "normal";
    }
    
    // Make sure the current element is visible in an expanded node in the toc. We ask it to expand 
    // the node above it, to ensure its child nodes are not expanded.
    var elClusterNewActivity = document.getElementById("div" + strActivityIdNew);
    //ExpandActivity( FindActivityId(elClusterNewActivity.parentElement) );
    ExpandActivity( FindActivityId(elClusterNewActivity.parentNode) );
    
    // Save the current activity id.
    g_previousActivityId = g_currentActivityId;
    g_currentActivityId = strActivityIdNew;
    
    return true;
}

// Something happened with the selection, so we need to reset the current element to the previous element
function ResetToPreviousSelection()
{
    SetCurrentElement(g_previousActivityId);
}

function body_onclick(e)
{   
   var e = e || window.event;

   var eSrc = e.target || e.srcElement;
            
    var nameSelected = (eSrc.tagName == "A");
    
    // Find the activity id corresponding to the clicked element
    var strActivityId = FindActivityId(eSrc);
    
    // If the activity id could be determined from the event and the activity 
    // is clickable, then process the event. 
    if (!eSrc.disabled && (strActivityId != undefined))
    {
        // The cluster div contains just this activity
        var elCluster = document.getElementById("divCluster" + strActivityId);
       
        // If the cluster has children, then determine whether to show an activity or 
        // show/hide the cluster
        if (IsParentElement ( elCluster ) )
        {
            // The element was a parent element. If the icon (not name link) was selected, 
            // then expand / collapse group. If the name was selected, the if that
            // element is valid for choice navigation, make that the current activity.
            if (nameSelected)
            {
                // If the node is valid for choice navigation, move to it.
                if (IsValidChoice( strActivityId ))
                {
                    MoveToActivity( strActivityId );
                }
            }
            else
            {
                // Icon selected. Just expand / collapse group.
                ExpandCollapseGroup( strActivityId, elCluster, false );                
            }
        }
        else
        {   
            // The cluster has only the selected activity, so if it's valid for choice, then 
            // move to that activity.
            if (IsValidChoice( strActivityId ))
            {         
                MoveToActivity( strActivityId );
            }
        }
    }
    
   e.cancelBubble = true;
   e.returnValue = false;
}

// Expands or collapses the group associated with strActivityId.
// If bExpandOnly is true, the group is not collapsed.
function ExpandCollapseGroup( strActivityId, elCluster, bExpandOnly )
{
    var elClusterImg = document.getElementById("icon" + strActivityId);
    if (elCluster.style.display == "none")
    {
        // Currently not shown. Expand the child div.
        elCluster.style.display = "block";

        if (elClusterImg != undefined)
        {
            elClusterImg.src = "Theme/MinusBtn.gif";
        }
    }
    else
    {
        // Currently shown. Hide the child if bExpandOnly is false
        if (!bExpandOnly)
        {
            elCluster.style.display = "none";          
            
            if (elClusterImg != undefined)
            {
                elClusterImg.src = "Theme/PlusBtn.gif";
            }
        }     
    }
}

function MoveToActivity ( strActivityId )
{
    if (SetCurrentElement( strActivityId ))
    {
        // The current activity has changed, so update the content frame.
        var elA = document.getElementById("a" + strActivityId);
        if (elA != undefined)
        {
            g_frameMgr.DoChoice( strActivityId, false /* dontClearContent */, true /* forceReloadCurrentActivity */ );
        }
    }
}

// Recursive function that returns true if elCluster has children (at any depth) that are <a> nodes.
function IsParentElement( elCluster )
{
    if (elCluster == undefined)
        return false;
    
    var linkChildren = elCluster.getElementsByTagName("A");
    return (linkChildren.length != 0);
}

// Returns true if the activity has a resource
function IsValidChoice( strActivityId )
{
    if (strActivityId == undefined)
    {
        return false;
    }
        
    // If the frameset mgr is not ready for a navigation command, then just 
    // eat this one.
    if (!g_frameMgr.ReadyForNavigation())
    {
        return false;
    }
        
    var divParent = document.getElementById("div" + strActivityId);
    return (divParent.attributes["isValidChoice"].nodeValue == "true");
}

// Sets the disable / or not state in each toc node.
function SetTocNodes( strNodeStates )
{
    // format of strNodeStates:
    //  strActivityId,true;fActivityId,false;
    //  where strActivityId is the numeric activity id
    //      if (false) the node should be disabled
    var activityPairs = strNodeStates.split(";");
    var numPairs = activityPairs.length;
    var aActivityDisabled;
    var i;
    for (i=0; i<numPairs; i++)
    {
        if (activityPairs[i] == "")
            continue;
        
        aActivityDisabled = activityPairs[i].split(",");
        isDisabled = (aActivityDisabled[1] == "false");
        
        var elActivity = document.getElementById("a" + aActivityDisabled[0]);
        if (elActivity != undefined)
        {
            elActivity.disabled = isDisabled;
            elActivity.className = isDisabled ? "disable" : "enable";
        }
                
        var elDiv = document.getElementById("div" + aActivityDisabled[0]);
        if (elDiv != undefined)
        {
            elDiv.setAttribute("isValidChoice", isDisabled ? "false" : "true");
        }
    }
}

