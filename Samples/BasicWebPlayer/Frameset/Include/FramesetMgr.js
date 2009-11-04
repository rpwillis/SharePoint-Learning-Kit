// Copyright (c) Microsoft Corporation. All rights reserved.

// FramesetManager receives communication from various frames and passed it on to the 
// other frames or the Site object. Functions that start with FM_ are attached to the object. 
// Functions with API_ can be called directly. Other functions should not be called from outside this file.

// A comment on naming convention in this file.
// Any function that starts with FM_ is a member function on the FramesetManager, and therefore has a 'this' value.
// Functions that do not start with FM_ are *not* member functions on FramesetManager and probably do not have a 'this' value.

// Constants: Frame names
CONTENT_FRAME = "frameContent";
HIDDEN_FRAME = "frameHidden";
NAVCLOSED_FRAME = "frameNavClosed";
NAVOPEN_FRAME = "frameNavOpen";
TITLE_FRAME = "frameTitle";
TOC_FRAME = "frameToc";
MAIN_FRAME = "frameLearnTask";
PARENTUI_FRAMESET = "framesetParentUI"

// Commands
CMD_NEXT = "N";
CMD_PREVIOUS = "P";
CMD_SAVE = "S"; // aka. Commit
CMD_TERMINATE = "T";
CMD_CHOICE = "C";
CMD_TOC_CHOICE = "TC";  // The TOC is choosing an activity
CMD_IS_CHOICE_VALID = "V";  // Is selecting an activity valid?
CMD_IS_NAV_VALID = "NV";     // Is non-choice nav command valid?
CMD_DO_SUBMIT = "DS";   // Do the submission of the attempt

var L_ServerErrorTitle_TXT = "Unable to Process Request";
var L_FormSubmitFailed_TXT = "The page could not be saved because one or more answers is invalid. If you are attaching a file, verify that the file path is correct."

function FindFrmMgr(win)
{
   var frmDepthCount = 0;
   while ((win.g_frameMgr == null) && (win.parent != null) && (win.parent != win))
   {
      frmDepthCount++;
      
      if (frmDepthCount > 20) 
      {
         return null;
      }
      
      win = win.parent;
   }
   return win.g_frameMgr;
}


// Recurse up the frame hierarchy to find the FramesetManager object.
function API_GetFramesetManager()
{
    return FindFrmMgr(window);
}

function FramesetManager()
{
    // "public" api
    this.GetRteApi = FM_GetRteApi;
    this.RegisterFrameLoad = FM_RegisterFrameLoad;
    this.DoNext = FM_DoNext;
    this.DoPrevious = FM_DoPrevious;
    this.DoChoice = FM_DoChoice;
    this.Cancel = FM_Cancel;
    this.Terminate = FM_Terminate;
    this.Save = FM_Save;
    this.IsNavigationValid = FM_IsNavigationValid;  // request to see if navigation is valid
    this.CommitDataModel = FM_CommitDataModel;  // called by the rteSite to send data to the server, requires passing the datamodel values
    this.ShowActivityId = null; // value set by toc frame
    this.ResetActivityId = null; // value set by toc frame
    this.SetTocNodes = null; // value set by toc frame
    this.ClearContentFrameAndPost = FM_ClearContentFrameAndPost;
    this.ContentIsCleared = FM_ContentIsCleared;    // callback when content frame has been cleared
    this.SetTitle = FM_SetTitle;    // Saves the title value. Must call UpdateTitle to make it change.
    this.SetNavVisibility = FM_SetNavVisibility;    // Sets the nav visibility state. Must call UpdateNavVisibility to make it change.
    this.SetContentFrameUrl = FM_SetContentFrameUrl;    // Sets the url that will be loaded into the content frame.
    this.SetPostFrame = FM_SetPostFrame;    // Sets the frame to post when post is required. Call DoPost to post the frame.
    this.SetView = FM_SetView;  // Sets the view of the current session. 
    this.SetAttemptId = FM_SetAttemptId;
    this.SetActivityId = FM_SetActivityId;  // Set the id of the current activity
    this.ShowErrorMessage = FM_ShowErrorMessage;    // Show error message in content frame
    this.SetIsNavigationValid = FM_SetIsNavigationValid;   // update with server response re: is navigation valid
    this.SetDataModelValues = FM_SetDataModelValues;    // does not commit them
    this.SetErrorMessage = FM_SetErrorMessage;  // Set an error message to be show when frames are loaded.
    this.InitDataModelValues = FM_InitDataModelValues;
    this.InitNewActivity = FM_InitNewActivity;  // called when a new activity has been loaded -- reinitialize the RTE if needed
    this.TrainingComplete = FM_TrainingComplete;    // sets messages to display because training is not complete
    this.IsTrainingComplete = FM_IsTrainingComplete;    // returns true if training is done
    this.SetPostableForm = FM_SetPostableForm;
    this.PostIsComplete = FM_PostIsComplete;    // Indicates an in-process post operation has finished
    this.DoSubmit = FM_DoSubmit;    // submit the attempt as complete
    this.IsClosing = FM_IsClosing;  // returns true if the frameset is closing.
    this.getContentFrame = GetContentFrame;
    this.getHiddenFrame = GetHiddenFrame;
    
    // Frames that have registered as being loaded
    this.m_framesRegistered = new Array();
    //this.m_framesRegistered[CONTENT_FRAME] = false;   -- don't have to register this frame
    this.m_framesRegistered[HIDDEN_FRAME] = false;
    this.m_framesRegistered[TOC_FRAME] = false;
    this.m_framesRegistered[NAVOPEN_FRAME] = false;
    this.m_framesRegistered[NAVCLOSED_FRAME] = false;
    this.m_framesRegistered[TITLE_FRAME] = false;
    
    // The RteSite object to communicate with RTE
    this.m_rteSite = new RteApiSite( this );
    
    // "private" api to support the object functionality
    this.MakeFramesVisible = FM_MakeFramesVisible;
    this.DoPost = FM_DoPost;
    this.HasContentError = FM_HasContentError;
    
    // private methods to handle events 
    this.RegisterReadyStateChange = FM_RegisterReadyStateChange_ContentFrame;   // register for readystate change events
    this.WaitForContentCompleted = FM_WaitForContentCompleted;   // wait for a specified number of times that the content frame loading is complete
    this.ReadyForNavigation = FM_ReadyForNavigation; // returns true if the frameset manager is in a state that accepts navigation commands
    this.ReadyStateReceived = FM_ReadyStateReceived;    // decrements the number of times to wait for the content frame
            
    // "private" data for the object
    this.m_title = "&nbsp;";    // title to display in title frame
    this.m_showNext = false;
    this.m_showPrevious = false;
    this.m_showAbandon = false;
    this.m_showExit = false;
    this.m_showSave = false;
    this.m_contentFrameUrl = null;  // url to load in content frame
    this.m_postFrameName = null;    // the name of the frame to be posted when a post is required
    this.m_view = 0;
    this.m_attemptId = "-1";
    this.m_activityId = "-1";
    this.m_dataModelValues = null;  // string containing data model values to post
    this.m_objectiveIdMap = null;   // map n to id for objectives. Only used to post back to server -- not used on client.
    this.m_contentFrameErrorTitle = null;   // title (text format) for error message display
    this.m_contentFrameErrorMessage = null; // error message (text format) to display in content frame
    this.m_contentFrameErrorAsInfo = false; // if true, display error as info message
    this.m_pendingCommand = null;
    this.m_pendingCommandData = null;
    this.m_isTrainingComplete = false;
    this.m_postableForm = null; // the form element that should be posted
    this.m_postInProgress = false;  // If true, we're waiting for a return from posted data. Nothing more can be posted.
    this.m_waitForContentCount = 0;
    this.m_isClosing = false;   // if true, the frameset is closing
    
    // To process commands
    this.m_commandMgr = new CommandMgr();
    
    // Debug log
    // FOR CHECKED-IN CODE, this should always be assigned to NoOp
    this.m_log = new Log();
    //this.DebugLog = FM_DebugLog;
    this.DebugLog = FM_NoOp;
}

function FM_NoOp()
{
    // do nothing
}

function FM_DebugLog( strMessage )
{
    this.m_log.WriteMessage( strMessage );
}

// Get the api object for the specified scormVersion
// rteRequired is true if the first activity requires the rte.
function FM_GetRteApi( scormVersion, rteRequired )
{
   return this.m_rteSite.GetRteApi(scormVersion, rteRequired );
}

// Register the frameName as the frame to be posted to the server when 
// a post is required.
function FM_SetPostFrame(frameName)
{
    this.m_postFrameName = frameName;
}

// Register that the frame has loaded. 
function FM_RegisterFrameLoad(frameName)
{
    this.m_framesRegistered[frameName] = true;
    if (AllFramesRegistered(this.m_framesRegistered))
    {           
        // If the training has been completed, then disable UI controls and show the message to the user.
        if (this.IsTrainingComplete())
        {
            HideUIControls();
            this.ShowErrorMessage( this.m_contentFrameErrorTitle, this.m_contentFrameErrorMessage, true );
        }
        else
        {
            // Register for ready state changes in content frame
            this.RegisterReadyStateChange();
            
            // Update all the ui controls
            UpdateNavVisibility(this.m_showNext, this.m_showPrevious, this.m_showAbandon, this.m_showExit, this.m_showSave);
            UpdateTitle(this.m_title);
            
            this.ShowActivityId( this.m_activityId );    // tell TOC about the activity
            
            // If there was an error in server processing, don't render content frame. Instead, show error message.
            if (this.m_contentFrameErrorMessage != null)
            {
                this.ShowErrorMessage( this.m_contentFrameErrorTitle, this.m_contentFrameErrorMessage, this.m_contentFrameErrorAsInfo );
            }
            else
            {   
                LoadContentFrame( this.m_contentFrameUrl );
            }
        }
        // Show the ui controls
        this.MakeFramesVisible();
    }
}


// Event handler for readyStateChangeEvent.
function OnReadyStateChange_ContentFrame()
{
    // Note that this is IE-specific behavior and would require retesting to verify support other browsers
    
    g_frameMgr.DebugLog("OnReadyStateChange: Begin");        
    g_frameMgr.DebugLog("\tGetContentFrame().readyState = " + GetContentFrame().readyState);
    if (GetContentFrame().readyState == "complete")
        g_frameMgr.ReadyStateReceived();
}

// Register for the ready state change event on the window.
function FM_RegisterReadyStateChange_ContentFrame()
{
    /* readystatechange is IE specific, so only wire up the event for IE.
     * For other browers decrement m_waitForContentCount as this is only called from RegisterFrameLoad anyway
     */

    var contentFrame = GetContentFrame();
    if (contentFrame.onreadystatechange != undefined)
    {
        GetContentFrame().onreadystatechange = OnReadyStateChange_ContentFrame;
    }
    else
    {
        g_frameMgr.ReadyStateReceived();
    }
}

// Wait for the specified number of times the content frame is loaded before allowing another navigation request
function FM_WaitForContentCompleted( iCount )
{
    this.DebugLog("WaitForContentCompleted: Begin. Count = " + iCount); 
    if (!this.ReadyForNavigation() && (iCount > 0))
        throw "Asking to wait for completion when it's not ready for navigation";
        
    this.m_waitForContentCount = iCount;
    this.DebugLog("WaitForContentCompleted: End. m_waitForContentCount = " + this.m_waitForContentCount); 
}

function FM_ReadyStateReceived()
{
    this.DebugLog("ReadyStateReceived: Begin. m_waitForContentCount = " + this.m_waitForContentCount); 
    
    // Decrement the number of times we are waiting, only if we are actually waiting
    if (this.m_waitForContentCount > 0)
        --this.m_waitForContentCount;
        
    this.DebugLog("ReadyStateReceived: End. m_waitForContentCount = " + this.m_waitForContentCount); 
}

// Returns true if the object is ready to accept navigation commands
function FM_ReadyForNavigation()
{
    this.DebugLog("ReadyForNavigation: Begin. m_waitForContentCount = " + this.m_waitForContentCount); 
    var isReady = (this.m_waitForContentCount <= 0);
    this.DebugLog("ReadyForNavigation: End. isReady = " + isReady); 
    return isReady;
}
    
// Note that the training has been completed. The message should be displayed.
function FM_TrainingComplete(strTitle, strMessage)
{
    this.m_contentFrameErrorTitle = strTitle;
    this.m_contentFrameErrorMessage = strMessage;
    this.m_contentFrameErrorAsInfo = true;
    this.m_isTrainingComplete = true;
}

// Returns true if the training has been completed.
function FM_IsTrainingComplete()
{
    return this.m_isTrainingComplete;
}

// Hides the UI controls frameset. 
function HideUIControls()
{
    window.top.frames[MAIN_FRAME].document.getElementById("framesetParentUI").cols = "0px,*";
    
    var titleDoc = window.top.frames[TITLE_FRAME].document;
    titleDoc.getElementById("imgSaveAndCloseTd").innerHTML = "&nbsp;";
    titleDoc.getElementById("aSaveAndClose").innerHTML = "&nbsp;";
}

// Returns true if all frames have been registered as loaded.
function AllFramesRegistered(framesRegistered)
{
    return ( framesRegistered[HIDDEN_FRAME] 
        && framesRegistered[TITLE_FRAME]
        && framesRegistered[TOC_FRAME] 
        && framesRegistered[NAVOPEN_FRAME]
        && framesRegistered[NAVCLOSED_FRAME]);
}

// Return the document in the Title frame.
function GetTitleDoc()
{
    return document.getElementById(TITLE_FRAME).contentWindow.document;
}

function GetTocDoc()
{   
    return window.top.frames[MAIN_FRAME].document.getElementById(TOC_FRAME).contentWindow.document;
}

function GetContentFrame()
{
     return window.top.frames[MAIN_FRAME].document.getElementById(CONTENT_FRAME);
}

function GetNavFrame( navFrameName )
{
    return window.top.frames[MAIN_FRAME].document.getElementById(navFrameName);    
}


function GetHiddenFrame()
{
    return window.top.frames[MAIN_FRAME].document.getElementById(HIDDEN_FRAME);    
}


// Set all values in the various frames and then make them visible.
function FM_MakeFramesVisible()
{
    // Make title and toc visible
    GetTitleDoc().getElementById("txtTitle").style.display = "block";
    GetTocDoc().getElementById("divMain").style.visibility = "visible";
}

// Save the value to set the title string to. Later, call UpdateTitle to change the contents of the frame.
function FM_SetTitle(title)
{
    this.m_title = title;
}


// Update the title string in the Title Frame.
function UpdateTitle(title)
{
    var titleDoc = GetTitleDoc();
    titleDoc.getElementById("txtTitle").innerHTML = title;
}

// Returns true if the frameset is closing.
// If isClosing has a value, this sets its value.
function FM_IsClosing(isClosing)
{
    if (isClosing != undefined)
        this.m_isClosing = isClosing;
        
    return this.m_isClosing;
}

// strCommand is the pending navigation request that should be executed after the 
// content frame has been cleared.
function FM_ClearContentFrameAndPost( command, commandData )
{    
    this.DebugLog("ClearContentFrameAndPost: Begin");
    this.DebugLog("\tCommand: " + command + " CommandData: " + commandData);
    // If hidden frame is the one being posted, then clear the content frame first. Otherwise, just
    // continue with posting the content frame.
    if (this.m_postFrameName == HIDDEN_FRAME)
    {
        // Wait for the content frame to load twice before allowing a navigation command
        g_frameMgr.WaitForContentCompleted(2);
        
        // Save the command information in case the content frame triggers more commands
        this.m_pendingCommand = command;
        this.m_pendingCommandData = commandData;
        this.Save();    // save any unsaved data from RTE
        LoadContentFrame( "./ClearContent.aspx" );
    }
    else
    {
        g_frameMgr.WaitForContentCompleted(2);
        this.m_commandMgr.SetCommand(command, commandData);
        this.DoPost();
    }
    this.DebugLog("ClearContentFrameAndPost: End");
}

// Callback from ClearContent.htm to indicate it's time to send info to server
// Returns false if there was no pending command data. This indicates an error condition 
// such as the user clicking back button to get to this page, so clearcontent page will
// show an error. In other cases, return true.
function FM_ContentIsCleared()
{
    this.DebugLog("ContentIsCleared: Begin");
    if (this.m_pendingCommand == null)
    {
        this.DebugLog("ContentIsCleared: End. No pending command.");
        return false;
    }
    
    this.DebugLog("m_postInProgress = " + this.m_postInProgress);
    
    // If the frameset is closing and we are not waiting for anything from the server, then close the window    
    if (this.IsClosing() && !this.m_postInProgress)
    {
        top.close();
    }
    
    this.m_commandMgr.SetCommand(this.m_pendingCommand, this.m_pendingCommandData);
    
    this.DoPost();  // post, not save, since there is no RTE object at this point
    
    this.m_pendingCommand = null;
    this.m_pendingCommandData = null;
    
    this.DebugLog("ContentIsCleared: End.");
    return true;
} 

function FM_DoNext()
{
    this.DebugLog("DoNext command received");
    if (!this.ReadyForNavigation())
    {
        this.DebugLog("DoNext command ignored.");
        return;    
    }    
    this.ClearContentFrameAndPost( CMD_NEXT );
    this.DebugLog("DoNext command processed");
}

function FM_DoPrevious()
{
    this.DebugLog("DoPrevious command received");
    if (!this.ReadyForNavigation())
    {
        this.DebugLog("DoPrevious command ignored");
        return;
    }
    this.ClearContentFrameAndPost( CMD_PREVIOUS );
}

// if bDoNoClearContentFrame is true, the content frame is not cleared, but the post is 
// This is an uncommon case when LRM content is being redirected to a new 
// activity. In this case, the hidden frame is being posted, even though 
// content is LRM.
// if bReinitializeCurrentActivity is true, then even if the current activity is the 
// same as the selected one, the activity is reinitialized. This should only be set to true
// by the table of contents.
function FM_DoChoice(activityId, bDoNotClearContentFrame, bReinitializeCurrentActivity )
{
    this.DebugLog("DoChoice command received. ActivityId = " + activityId);
    if (bDoNotClearContentFrame == true)
    {
        // If the request was to not clear the content frame, then just do the post
        // This is an uncommon case when LRM content is being redirected to a new 
        // activity. In this case, the hidden frame is being posted, even though 
        // content is LRM.
        this.WaitForContentCompleted(1);
        this.m_commandMgr.SetCommand( CMD_CHOICE, activityId );
        this.DoPost();
    }
    else
    {
        var command = CMD_CHOICE;
        if (bReinitializeCurrentActivity == true)
            command = CMD_TOC_CHOICE;
            
        // Clear the content frame and then post.
        this.ClearContentFrameAndPost( command, activityId );
    }
}

function FM_DoSubmit()
{
    this.m_commandMgr.SetCommand( CMD_DO_SUBMIT );
    this.DoPost();
}

function FM_Cancel()
{
}

function FM_HasContentError()
{
    if (this.m_contentFrameErrorMessage == null)
        return false;
        
    return true;
}

function FM_Terminate( )
{
    this.DebugLog("Terminate: Begin");
    // If terminate is being called when we are displaying a content error, it's likely this 
    // is an onunload handler, in which case we ignore it.
    if (!this.HasContentError())
    {
        this.m_commandMgr.SetCommand(CMD_TERMINATE);
        this.DoPost();
    }
    this.DebugLog("Terminate: End");
}

function FM_Save()
{
    this.DebugLog("Save: Begin");
    var dataSaved = false;
    // If there is an api object, ask it to commit data
    if (this.m_rteSite.MlcRteApi != null)
    {
        // Ask the site (not the RTE) to commit so that the RTE state 
        // does not interfere with the ability to save data 
        this.m_rteSite.Commit("");
        dataSaved = true;
    }
    
    if (!dataSaved)
    {
        this.m_commandMgr.SetCommand(CMD_SAVE);
        this.DoPost();
    }
    this.DebugLog("Save: End");
}

// strCommand is either CMD_IS_CHOICE_VALID or CMD_IS_NAV_VALID. The strCommandData is either the CMD (eg, CMD_NEXT) or the 
// activityid being requested.
function FM_IsNavigationValid( strCommand, strCommandData )
{
    this.m_commandMgr.SetCommand( strCommand, strCommandData );
    this.DoPost();
}

// strNavResponse is of the form command@Evalue@N -- where command is either "N", "P" or "C,<strActivityId>".
function FM_SetIsNavigationValid( strNavResponse )
{
    // Tell the api site the response
    this.m_rteSite.SetValidNavigationCommands( strNavResponse );
} 

// Update the data model values on the server. The values string is not processed by
// this function. It is simply passed to the server. This function returns true if the data 
// was posted to the server. 
function FM_CommitDataModel( strDataModelValues )
{
    this.DebugLog("CommitDataModel: Begin");
    // If the view is Review, then return false (not saved)
    if (this.m_view == "3")
    {
        this.DebugLog("CommitDataModel: End. Review view.");
        return false;
    }
    
    // Happily do nothing if there's nothing to save.
    if (strDataModelValues.length == 0)
    {
       this.DebugLog("CommitDataModel: End. Nothing to save. Data not posted.");
        return false;
    }
       
    // We add the strDataModelValues to the m_dataModelValues in case there are going 
    // to make multiple commit calls within one post to the server.
    if (this.m_dataModelValues == null)
        this.m_dataModelValues = strDataModelValues;
    else 
        this.m_dataModelValues += strDataModelValues;
        
    this.m_commandMgr.SetCommand( CMD_SAVE );
    
    var didPost = false;    // true if data is posted
    didPost = this.DoPost();
    
    this.DebugLog("CommitDataModel: End. Data posted.");
    
    return didPost;
}

// Set the values that will update the UI navigation controls
function FM_SetNavVisibility ( showNext, showPrevious, showAbandon, showExit, showSave)
{
    this.m_showNext = showNext;
    this.m_showPrevious = showPrevious;
    this.m_showAbandon = showAbandon;
    this.m_showExit = showExit;
    this.m_showSave = showSave;
}


// Update the frames that display the navigation UI. (Currently, abandon and exit are ignored.)
function UpdateNavVisibility ( showNext, showPrevious, showAbandon, showExit, showSave )
{
    var frame = GetNavFrame ( NAVOPEN_FRAME );
    var navDoc = frame.contentWindow.document;
    
    SetVisibility(navDoc.getElementById("divNext"), showNext);
    SetVisibility(navDoc.getElementById("divPrevious"), showPrevious);
    SetVisibility(navDoc.getElementById("divSave"), showSave);
    
    frame = GetNavFrame ( NAVCLOSED_FRAME );
    navDoc = frame.contentWindow.document;
    
    SetVisibility(navDoc.getElementById("divNext"), showNext);
    SetVisibility(navDoc.getElementById("divPrevious"), showPrevious);
    SetVisibility(navDoc.getElementById("divSave"), showSave);
}

// Helper function to set the visibility style of a div based on showUI.
function SetVisibility( div, showUI)
{
    var visibility;
    
    if (showUI)
        visibility = "visible";
    else
        visibility = "hidden";
        
    div.style.visibility = visibility;
}

// Set the url of the content frame. To load the url into the frame, call LoadContentFrame().
function FM_SetContentFrameUrl( url )
{
    this.DebugLog("SetContentFrameUrl: Begin. Url = " + url);
    this.m_contentFrameUrl = url;
    this.DebugLog("SetContentFrameUrl: End");
} 

// Do a GET request into the content frame.
function LoadContentFrame ( url )
{
    g_frameMgr.DebugLog("SetContentFrameUrl: Begin. Url = " + url);
    if ((url == null) || (url == undefined) || (url == ""))
        return;
       
    var fr = GetContentFrame();
    fr.contentWindow.location.href =url;
    g_frameMgr.DebugLog("SetContentFrameUrl: End. Navigation complete. ");
}

// Sets (does not commit) data model values
function FM_SetDataModelValues ( strDataModelValues )
{
    this.m_dataModelValues = strDataModelValues;
}

function FM_SetPostableForm( formToPost )
{
    this.DebugLog("SetPostableForm: Begin");
    this.DebugLog("\tAction: " + ((formToPost != null) ? formToPost.action : "is null"));
    this.m_postableForm = formToPost;
}

var g_retryCount = 0;
var MAX_RETRY = 50;

// Set the values in hidden fields in m_postableForm, then post that frame.
// Returns true only if the form is submitted.
function FM_DoPost( bIsRetry )
{
    this.DebugLog("DoPost: Begin. isRetry = " + bIsRetry);
    
    // If there is already a post in progress, then do nothing right now.
    if ((this.m_postInProgress) && (bIsRetry != true))
    {
        this.DebugLog("DoPost: End. Post in progress.");
        return false;
    }
    this.m_postInProgress = true;
    
    // If this.m_postableForm isn't set (as can happen in some race conditions with hidden and 
    // content frames), try to get the form from the m_frameName
    var form = this.m_postableForm;
    
    if (form == undefined)
    {
        try
        {
            form = window.top.frames[MAIN_FRAME].document.getElementById(this.m_postFrameName).contentWindow.document.forms[0];
        }
        catch (e) { 
            // do nothing
        }
    
        // If the form is still not defined, it probably indicates the frame is still loading. In this case,
        // it obviously cannot be posted, so wait a bit of time and try again.            
        if ((form == undefined) || (form == null))
        {
            // This probably indicates the frame is still loading, in which case we obviously cannot post it.
            g_retryCount++;
            if (g_retryCount < MAX_RETRY)
            {
                setTimeout("API_GetFramesetManager().DoPost( true )", 500); 
                this.DebugLog("DoPost: End. Try again.");
            }
            else
            {
                this.DebugLog("DoPost: End. ** MAX RETRY count exceeded.");
            }
            return false;            
        }
    }
    g_retryCount = 0;
        
    this.DebugLog("\tDoPost: commands: " + this.m_commandMgr.GetCommands());
    this.DebugLog("\tDoPost: commandData: " + this.m_commandMgr.GetCommandData());
    this.DebugLog("\tDoPost: dataModelValues: " + this.m_dataModelValues);
    // Set the values in hidden controls
    SetHiddenControl( form, "hidCommand", this.m_commandMgr.GetCommands());
    SetHiddenControl( form, "hidCommandData", this.m_commandMgr.GetCommandData());
    SetHiddenControl( form, "hidAttemptId", this.m_attemptId);
    
    if (this.m_view == 0)   // Only post data if this is Execute view.
    {
        SetHiddenControl( form, "hidDataModel", this.m_dataModelValues );
        SetHiddenControl( form, "hidObjectiveIdMap", this.m_objectiveIdMap );
    }
       
    var formSubmitted = false;
    try
    {
        // Submit the form
        form.submit();    
        formSubmitted = true;
    }
    catch(e)
    {
        alert(L_FormSubmitFailed_TXT);
        
    }
    
    if (formSubmitted)
    {
        // Clear memory of submitted data
        this.m_dataModelValues = null;
        this.m_command = null;
        this.m_commandData = null;
        this.m_objectiveIdMap = null;
        this.m_contentFrameErrorTitle = null;
        this.m_contentFrameErrorMessage = null;
        this.m_contentFrameErrorAsInfo = false;
        this.m_framesRegistered[this.m_postFrameName] = false;
        this.m_commandMgr.ClearCommands();
    }
    else
    {
        this.m_postInProgress = false;
        this.WaitForContentCompleted(0);    // not waiting for anything
        this.ResetActivityId(); // fix TOC to show the activity id before this request
    }
    
    this.DebugLog("DoPost: End. Form submitted. Form action: " + form.action);
    return formSubmitted;
}

function FM_PostIsComplete()
{
    this.DebugLog("PostIsComplete: Begin");
    
    this.m_postInProgress = false;
    
    // If there was an error in the posted data, don't do the next post
    if (this.HasContentError())
    {
        if (this.IsClosing())
        {
            // If there is nothing waiting to be posted, then close the window if that is a pending command.
            top.close();
        }
        else
            return;
    }
        
    if (this.m_commandMgr.HasCommands())  // If there are more things to post, do it
    {
        this.DoPost();
    }
    else if (this.IsClosing())
    {
        // If there is nothing waiting to be posted, then close the window if that is a pending command.
        top.close();
    }
    this.DebugLog("PostIsComplete: End");
}

// Set the hidden control with the specified 'ctrlName' to have the 'ctrlValue'. 
// If the control doesn't already exist in the form, create it.
function SetHiddenControl( form, ctrlName, ctrlValue )
{
    //var ctrl = form.item( ctrlName );
    var ctrl = form[ctrlName];
    
    if ( ctrl == null )
    {
        var elInput = form.ownerDocument.createElement("input");
        elInput.type = "hidden";
        elInput.id = ctrlName;
        elInput.name = ctrlName;
        if (ctrlValue == null)
            elInput.value = "";
        else
            elInput.value = ctrlValue;
        form.appendChild( elInput );
    }
    else
    {
        if (ctrlValue == null)
            ctrl.value = "";
        else
            ctrl.value = ctrlValue;
    }
}

function FM_SetView( view )
{
    this.m_view = view;
}

function FM_SetAttemptId ( strAttemptId )
{
    this.m_attemptId = strAttemptId;
}

function FM_SetActivityId ( strActivityId )
{
    this.DebugLog("SetActivityId: Begin. strActivityId = " + strActivityId);
    this.m_activityId = strActivityId;
}

// Store the new data model values. Setting this will clear any existing data model values.
// See the RteSiteApi definition for explaination of encoding of strDataModelValues.
function FM_InitDataModelValues ( strDataModelValues, strObjectiveIdMap )
{
    this.DebugLog("InitDataModelValues: Begin. strDataModelValues = " + strDataModelValues);
    this.m_rteSite.InitDataModelValues( strDataModelValues );
    this.m_objectiveIdMap = strObjectiveIdMap;
}

// Reset internal state to allow identifying a new activity
// rteRequired is true if the new activity requires the RTE
function FM_InitNewActivity( rteRequired ) 
{
    this.m_rteSite.Init( rteRequired );
}

// Store the error message (html encoded) to display to the user as a result of processing 
// data on the server.
function FM_SetErrorMessage ( strMessageHtml, strTitleHtml, bAsInfo )
{
    if (strTitleHtml == undefined)
    {
        this.m_contentFrameErrorTitle = L_ServerErrorTitle_TXT;
    }
    else
    {
        this.m_contentFrameErrorTitle = strTitleHtml;
    }  
      
    if (bAsInfo == undefined)
        this.m_contentFrameErrorAsInfo = false;
    else
        this.m_contentFrameErrorAsInfo = bAsInfo;
        
    this.m_contentFrameErrorMessage = strMessageHtml;
}

// Display the error message to the user in the content frame. This replaces anything currently in the content frame.
// bAsInfo: If false or undefined, show as error. If true, show as info.
function FM_ShowErrorMessage ( strTitleHtml, strMessageHtml, bAsInfo )
{
    // the html to write to the content frame
    var strContentHtml; 
    var imgFile = (bAsInfo) ? "Info.gif" : "Error.gif";
    
    strContentHtml = "<html>"
    strContentHtml += "<head><LINK rel=\"stylesheet\" type=\"text/css\" href=\"./Theme/Styles.css\"/>";
    strContentHtml += "<script src=\"./Include/FramesetMgr.js\"></script>";
    strContentHtml += "</head>";
    
    strContentHtml += "<body class=\"ErrorBody\">";
    strContentHtml += "<table border=\"0\" id=\"ErrorTable\">";
    strContentHtml += "<tr><td width=\"60\"><p align=\"center\"><img border=\"0\" src=\"./Theme/";
    strContentHtml += imgFile;
    strContentHtml += "\" width=\"49\" height=\"49\"></td>";
	strContentHtml += "<td class=\"ErrorTitle\">";
	strContentHtml += strTitleHtml;
	strContentHtml += "</td></tr>";
	strContentHtml += "<tr><td width=\"61\">&nbsp;</td><td><hr></td></tr>";
	strContentHtml += "<tr><td width=\"61\">&nbsp;</td><td class=\"ErrorMessage\">";
	strContentHtml += strMessageHtml;
	strContentHtml += "</td></tr></table>";
    strContentHtml += "</body>";
    strContentHtml += "</html>";
    
    // write the information to the content frame
    var contentFrame = GetContentFrame(); 
    var contentDoc = contentFrame.contentWindow.document;
    contentDoc.open();
    contentDoc.write ( strContentHtml );
    contentDoc.close();
    
    // Clear the state that indicates we are waiting for something
    this.WaitForContentCompleted(0);
}

/************** CommandManager object **********************/
// CommandMgr implements a queue for pending commands to be sent to the server.
// It properly encodes command data for posting.

function CommandMgr()
{
    // The "public" api for the object
    this.SetCommand = CM_SetCommand;    // Saves the command and its associated data. Some commands do not have data.
    this.GetCommands = CM_GetCommands;  // Gets the serialized string representing commands to send to the server.
    this.GetCommandData = CM_GetCommandData;    // Gets the serialized string representing command data to send to the server.
    this.HasCommands = CM_HasCommands;  // If true, there are commands remaining to be sent.
    this.ClearCommands = CM_ClearCommands;  // Clears the list of pending commands.

    // Private data
    this.m_commands = new Array();
    this.m_commandData = new Array();    
}

// Saves the command and its associated data. Some commands do not have data.
function CM_SetCommand( command, commandData )
{
    this.m_commands.push(command);
    
    if (commandData != undefined)
        this.m_commandData.push(commandData);
    else
        this.m_commandData.push(null);
}

// Gets the serialized string representing commands to send to the server.
// Commands are sent separated by semi-colons.
function CM_GetCommands()
{
    var i;
    var cmdsToSend = "";    // return value
    
    // Command data is encoded with @C separating each set of data
    for (i in this.m_commands)
    {
        cmdsToSend += this.m_commands[i] + ";";
    }
    
    return cmdsToSend;
}

// Gets the serialized string representing command data to send to the server.
// Command data is send with @C separating them.
function CM_GetCommandData()
{
    var i;
    var dataToSend = "";    // return value
    
    // Command data is encoded with @C separating each set of data
    for (i in this.m_commandData)
    {
        // Not all commands have associated data
        cmdData = this.m_commandData[i];
        if (cmdData != null)
            dataToSend += EncodeCommandData ( cmdData );
            
        dataToSend += "@C";
    }
    return dataToSend;
}

// If true, there are commands remaining to be sent.
function CM_HasCommands()
{
    return (this.m_commands.length > 0);
}

// Clears the list of pending commands.
function CM_ClearCommands()
{
    this.m_commands = new Array();
    this.m_commandData = new Array();
}

// Encode the data for being sent to the server. Returns the encoded value.
function EncodeCommandData( data )
{
    return data.replace(/@/g, "@A").replace(/</g, "@L").replace(/>/g, "@G");
}

/************** Log object **********************/
// Log implements a log of the frameMgr operations

function Log()
{
    this.WriteMessage = LOG_WriteMessage;
    this.GetConsole = LOG_GetConsole;
    
    this.m_console;
}

function LOG_WriteMessage( strMessage )
{
    var console = this.GetConsole();
    console.document.writeln("Message: " + strMessage);
    console.document.writeln("-------------------------");
}

function LOG_GetConsole()
{
    if ((this.m_console == null) || (this.m_console.closed))
    {
        this.m_console = window.open("", "console", "width=600,height=300,resizable,scrollbars=yes");
        this.m_console.document.open("text/plain");
    }
    return this.m_console;
}
