using System;
using System.Runtime.CompilerServices;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Object = UnityEngine.Object;

#if UNITY_EDITOR
using System.Text;
using UnityEditor;

/// <summary>
/// Provides utility methods for debugging, logging, and message handling within Unity.
/// This static class includes methods for logging messages, selecting and highlighting components,
/// and managing a log registry for organized log retrieval. Designed to be used as an extension
/// for Unity's Component class and standalone log utility methods.
/// </summary>
[InitializeOnLoad]
#endif
public static class DebugPrint
{

#if UNITY_EDITOR
    /// <summary>
    /// Encapsulates functionality for structured and formatted message handling in debugging contexts.
    /// This struct maintains a buffer for concatenating and formatting messages before printing them
    /// through the specified action, providing a mechanism for controlled and consecutive logging.
    /// </summary>
    struct PrintF
    {
        private const char newLine = '\n';

        /// <summary>
        /// Represents a StringBuilder object used to accumulate and build a collection of strings for debug output.<br/>
        /// This variable acts as a storage for formatted strings to be printed or cleared later.
        /// </summary>
        private StringBuilder pantalla;

        /// <summary>
        /// A delegate used for printing messages or objects to a specific output destination.
        /// The `print` variable serves as an invocation point for passing messages,
        /// helping to abstract debugging or logging operations.
        /// </summary>
        private System.Action<object> print;

        /// <summary>
        /// Indicates whether the internal data buffer contains any content.
        /// Returns true if the buffer has a length greater than zero, otherwise false.
        /// </summary>
        public bool LenghtChk => pantalla.Length > 0;

        /// <summary>
        /// Provides functionality to manage and format strings for debugging purposes, maintaining a StringBuilder
        /// to store content, which can be cleared, appended, and printed to a specified output action.
        /// </summary>
        public PrintF(Action<object> print) : this()
        {
            pantalla = new StringBuilder();
            this.print = print;
        }

        /// <summary>
        /// Adds the specified string to the internal buffer for later printing. If the buffer is not empty,
        /// a newline character is prepended before appending the string.
        /// </summary>
        /// <param name="palabra">The string to be added to the buffer.</param>
        public void Add(string palabra)
        {
            if (pantalla.Length != 0)
            {
                pantalla.Append(newLine);
                pantalla.Append(palabra);
            }
            else
                pantalla.Append(palabra);
        }

        /// <summary>
        /// Prints a detailed message to the debug console, optionally associated with a specific Unity component.
        /// </summary>
        /// <param name="obj">The message object to be printed.</param>
        /// <param name="component">The Unity component optionally associated with the logged message.</param>
        public void Print(ref StringBuilder str)
        {        
            if(!LenghtChk)
                return;
            
            str.Append(pantalla);
            Clear();
        }

        /// <summary>
        /// Logs a message, along with optionally linking it to a Unity component.
        /// This message is processed and displayed using Unity's Debug system.
        /// </summary>
        /// <param name="obj">The object or message to log.</param>
        /// <param name="component">The Unity component optionally associated with the log message.</param>
        public void Print()
        {
            if(!LenghtChk)
                return;
            
            print.Invoke(pantalla);
            Clear();
        }

        /// <summary>
        /// Clears the content of the internal StringBuilder instance, resetting it to an empty state.
        /// </summary>
        public void Clear()
        {
            pantalla.Clear();
        }
    }

    /// <summary>
    /// Stores the end times and associated messages for highlighted components,
    /// allowing temporary highlighting functionality to track and manage expiration.
    /// </summary>
    private static Dictionary<int, (DateTime date, string message, Color background, Color text)> _highlightEndTimes = new ();

    /// <summary>
    /// Represents a collection of UnityEngine objects that are queued for selection and highlighting.
    /// This is used internally to facilitate the selection and preview of objects in the Unity Editor.
    /// </summary>
    private static HashSet<Object> _toSelect = new();

    /// <summary>
    /// Represents the debug logging functionality for capturing and managing debug-level messages
    /// within the application, enabling structured handling of debug output.
    /// </summary>
    private static PrintF _debug = new(Debug.Log);

    /// <summary>
    /// A structured log handler used for managing and printing warning messages in the debugging process.
    /// </summary>
    private static PrintF _warning = new(Debug.LogWarning);

    /// <summary>
    /// Represents a static instance of the PrintF structure specifically configured to log error messages
    /// using the Unity Debug.LogError method.
    /// </summary>
    private static PrintF _error = new(Debug.LogError);

    /// <summary>
    /// Holds an instance of a <see cref="StringBuilder"/> used for composing and
    /// managing formatted string data. Primarily utilized within the debugging
    /// process to build and organize log messages before outputting them to
    /// the Unity console.
    /// </summary>
    private static StringBuilder _stringBuilder = new();

    /// <summary>
    /// Determines if the debug, warning, or error logging systems have entries that meet a specific length check condition.
    /// </summary>
    private static bool Chk => _debug.LenghtChk || _error.LenghtChk || _warning.LenghtChk;

    /// <summary>
    /// Provides a static utility class for enhanced debugging and logging functionalities within Unity.
    /// This class offers methods for logging messages, handling log events, and interacting with Unity's
    /// hierarchy to provide custom highlighting and selection capabilities. Additionally, it manages a
    /// registry for logs, facilitating organized retrieval and filtering of logged data.
    /// </summary>
    static DebugPrint()
    {
        EditorApplication.update -= UpdateHighlightTimes;
        EditorApplication.update += UpdateHighlightTimes;
        
        EditorApplication.hierarchyWindowItemOnGUI -= HierarchyHighlight_OnGUI;
        EditorApplication.hierarchyWindowItemOnGUI += HierarchyHighlight_OnGUI;
    }

    /// <summary>
    /// Represents an event that schedules actions to be executed in the next frame in the Unity Editor loop.
    /// </summary>
    private static event EditorApplication.CallbackFunction ExecuteInNextFrame
    {
        add => EditorApplication.delayCall += value;
        remove => EditorApplication.delayCall -= value;
    }

    /// <summary>
    /// Renders custom highlights in the Unity Hierarchy window for specific game object instances based on custom logic.
    /// This is triggered during the GUI rendering of the Unity Hierarchy.
    /// </summary>
    /// <param name="instanceID">The unique identifier of the game object being rendered in the hierarchy window.</param>
    /// <param name="selectionRect">The rectangle area used to represent the game object in the hierarchy window.</param>
    private static void HierarchyHighlight_OnGUI(int instanceID, Rect selectionRect)
    {
        if (Event.current.type != EventType.Repaint) return;
            
        if (ShouldHighlight(instanceID, out string message, out Color BKCol, out Color TextCol))
        {
            Rect BackgroundOffset = new Rect(selectionRect.position, selectionRect.size);
    
            EditorGUI.DrawRect(BackgroundOffset, BKCol);

            EditorGUI.LabelField(new Rect(selectionRect.position + new Vector2(2f, 0f), selectionRect.size), 
                message.Replace("\n", " - "), 
                new GUIStyle { normal = new GUIStyleState() { textColor = TextCol } });

            EditorApplication.RepaintHierarchyWindow();
        }
    }

    /// <summary>
    /// Adds the specified Unity Object to a selection queue and schedules the selection to be processed
    /// in the next frame if the queue was previously empty.
    /// </summary>
    /// <param name="obj">The Unity Object to add to the selection queue.</param>
    private static void Select(Object obj)
    {
        if (_toSelect.Count == 0)
            ExecuteInNextFrame += Select;

        _toSelect.Add(obj);
    }

    /// <summary>
    /// Selects and highlights one or more UnityEngine.Object instances within the Unity editor.
    /// Adds the specified object to a pending selection list and ensures it is highlighted in the editor.
    /// If the selection list is not empty, it processes the selection to highlight and ping all objects.
    /// </summary>
    /// <param name="obj">The instance of UnityEngine.Object to be selected and highlighted in the editor.</param>
    private static void Select()
    {
        var arraySelected = _toSelect.ToArray();

        Selection.objects = arraySelected;

        foreach (var selected in arraySelected)
        {
            EditorGUIUtility.PingObject(selected);
        }
        
        
        _toSelect.Clear();
    }

    /// <summary>
    /// Periodically checks and removes expired highlight entries from the internal dictionary of components
    /// being visually highlighted in the Unity Editor. This method is executed as part of the Editor update cycle
    /// and ensures that outdated highlights do not persist indefinitely.
    /// </summary>
    private static void UpdateHighlightTimes()
    {
        DateTime now = DateTime.UtcNow;

        List<int> expiredIDs = new();
        foreach (var kvp in _highlightEndTimes)
        {
            if (kvp.Value.Item1 <= now)
                expiredIDs.Add(kvp.Key);
        }

        foreach (var id in expiredIDs)
        {
            _highlightEndTimes.Remove(id);
        }
    }

    private static void Highlight(object obj, Component component, float duration, Color? background, Color? text)
    {
        if(!background.HasValue)
            background = Color.green;
        
        if(!text.HasValue)
            text = Color.red;
        
        int instanceID = component.gameObject.GetInstanceID();
        _highlightEndTimes[instanceID] = (DateTime.UtcNow.AddSeconds(duration), obj.ToString(), background.Value, text.Value);  // Almacena el ID con 100 frames de duración
    }
    
    
    /// <summary>
    /// Highlights and selects a Unity game object in the editor for a specified duration
    /// while optionally associating the object with its corresponding Component.
    /// </summary>
    /// <param name="obj">The object to log or describe during the selection process.</param>
    /// <param name="component">
    /// The associated Unity Component whose GameObject will be highlighted and selected. This is optional.
    /// </param>
    /// <param name="duration">
    /// The time in seconds for which the GameObject remains highlighted in the editor. Defaults to 2 seconds.
    /// </param>
    private static void AndSelect(object obj, Component component, float duration, Color? background, Color? text)
    {
        if (component != null)
        {
            Highlight(obj, component, duration, background, text);
            
            Select(component.gameObject);
        }
    }

    /// <summary>
    /// Highlights the specified component's GameObject in the Unity Editor hierarchy, opens its property editor,
    /// and registers the component's instance ID for a specified duration to maintain highlight state.
    /// </summary>
    /// <param name="obj">The object containing the message or data to be logged in association with the component.</param>
    /// <param name="component">The Unity Component whose GameObject will be highlighted and whose property editor will be opened. If null, no highlighting or editor opening occurs.</param>
    /// <param name="duration">The duration, in seconds, for which the component's GameObject will remain highlighted in the Unity Editor hierarchy.</param>
    private static void AndOpen(object obj, Component component, float duration, Color? background, Color? text)
    {
        if (component != null)
        {
            Highlight(obj, component, duration, background, text);
            
            Select(component.gameObject);
            EditorUtility.OpenPropertyEditor(component.gameObject);
        }
    }

    /// <summary>
    /// Combines and outputs logged messages from different log levels, including Debug, Warning, and Error, into a single
    /// consolidated log using a StringBuilder. After printing the combined log, the StringBuilder is cleared for reuse.
    /// </summary>
    private static void PrintCombinado()
    {
        _error.Print(ref _stringBuilder);
        _warning.Print(ref _stringBuilder);
        _debug.Print(ref _stringBuilder);
        
        Debug.Log(_stringBuilder);
        _stringBuilder.Clear();
    }

    /// <summary>
    /// Determines if a specified instance ID should be highlighted in the Unity Hierarchy,
    /// while retrieving an associated message if the instanceID is found.
    /// </summary>
    /// <param name="instanceID">The unique identifier of the object to check for highlight eligibility.</param>
    /// <param name="message">The output parameter that receives the associated message if the instance ID is found.</param>
    /// <returns>
    /// True if the instance ID is configured to be highlighted; otherwise, false.
    /// </returns>
    private static bool ShouldHighlight(int instanceID, out string message, out Color background, out Color text)
    {
        var b = _highlightEndTimes.TryGetValue(instanceID, out var value);
        message = value.message;
        background = value.background;
        text = value.text;
        return b;
    }
    
#endif

#region Runtime
    struct LogRegistry
    {
        public string message;
        public string stackTrace;
        public LogType LogType;
    }

    private static bool _enableRegister;

    private static ConcurrentQueue<LogRegistry> _logRegistries = new();

    /// <summary>
    /// Indicates whether the log message registration system is enabled or disabled.<br/>
    /// When enabled, it subscribes to the log message receiving mechanism; when disabled, it unsubscribes.
    /// </summary>
    public static bool EnableRegister
    {
        get => _enableRegister;
        set
        {
            if(value==_enableRegister)
                return;

            _enableRegister = value;

            if (_enableRegister)
            {
                LogMessageReceivedThreaded += OnLogMessageReceivedThreaded;
            }
            else
            {
                LogMessageReceivedThreaded -= OnLogMessageReceivedThreaded;
            }
        }
    }


    /// <summary>
    /// Event that allows subscribing to or unsubscribing from the log message reception mechanism
    /// for threaded operations in the Unity application.<br/>
    /// This event is triggered whenever a log message is received in a threaded context.
    /// </summary>
    public static event Application.LogCallback LogMessageReceivedThreaded
    {
        add
        {
            Application.logMessageReceivedThreaded += value;
        }
        remove
        {
            Application.logMessageReceivedThreaded -= value;
        }
    }

    /// <summary>
    /// Handles log messages received in a threaded manner and stores them in a concurrent queue for later retrieval.
    /// </summary>
    /// <param name="logString">The content of the log message.</param>
    /// <param name="stacktrace">The stack trace associated with the log message.</param>
    /// <param name="type">The type of log message (e.g., Error, Warning, Log).</param>
    private static void OnLogMessageReceivedThreaded(string logString, string stacktrace, LogType type)
    {
        _logRegistries.Enqueue(new LogRegistry(){message = logString, stackTrace = stacktrace, LogType = type});    
    }

    /// <summary>
    /// Formats a given component along with the specified message into a string representation.
    /// </summary>
    /// <param name="t">The object or message to append to the formatted output.</param>
    /// <param name="component">The Unity Component to include in the formatted output. Can be null.</param>
    /// <param name="memberName"></param>
    /// <param name="filePath"></param>
    /// <param name="lineNumber"></param>
    /// <returns>A string representation combining the specified object and component details.</returns>
    private static string FormatComponent(object t, 
        Component component, 
        string memberName,
        string filePath, 
        int lineNumber)
    {
        //<a href=\"Assets/A.cs\" line=\"2\">local file</a>

#if UNITY_EDITOR
        string infoLink = $"[<a href=\"{filePath}\" line=\"{lineNumber}\">{System.IO.Path.GetFileName(filePath)}:{lineNumber}-{memberName}</a>]";
#else
        string infoLink = $"[{System.IO.Path.GetFileName(filePath)}:{lineNumber}-{memberName}]";
#endif
        
        if(component!=null)
            return $"{infoLink}->{t}\n\t-{component}: {component?.GetInstanceID()}";
        
        return $"{infoLink}->{t}";
    }

    /// <summary>
    /// Attempts to dequeue a log entry from the internal log registry.
    /// This method is intended to retrieve console output.
    /// Ensure the EnableRegister property is set to true before using this method.
    /// </summary>
    /// <param name="message">The message content of the log entry retrieved.</param>
    /// <param name="stackTrace">The stack trace associated with the log entry.</param>
    /// <param name="logType">The type of log (e.g., Error, Warning, Log).</param>
    /// <returns>Returns true if a log entry was successfully dequeued; otherwise, false.</returns>
    public static bool TryDequeueLogRegistry(out string message, out string stackTrace, out LogType logType)
    {
        var ret = _logRegistries.TryDequeue(out var result);

        message = result.message;
        stackTrace = result.stackTrace;
        logType = result.LogType;

        return ret;
    }

    /// <summary>
    /// Logs an error message and associates the message with the specified component in the console.
    /// </summary>
    /// <param name="component">The component to associate with the log message.</param>
    /// <param name="obj">The object or message to log as an error.</param>
    public static void Error(this Component component, object obj, [CallerMemberName]string memberName = "", [CallerFilePath] string filePath = "" , [CallerLineNumber] int lineNumber = 0)
    {
#if DEBUG
        Debug.LogError(FormatComponent(obj, component, memberName, filePath, lineNumber), component);
#else
        if (EnableRegister)
            OnLogMessageReceivedThreaded(FormatComponent(obj, component, memberName, filePath, lineNumber), string.Empty, LogType.Error);
        #endif
    }

    /// <summary>
    /// Executes a LogWarning and associates the provided message to the console entry linked with the specified component.
    /// </summary>
    /// <param name="obj">The object to log as a warning message.</param>
    /// <param name="component">The component associated with the message.</param>
    public static void Warning(this Component component, object obj, [CallerMemberName]string memberName = "", [CallerFilePath] string filePath = "" , [CallerLineNumber] int lineNumber = 0)
    {
#if DEBUG
        Debug.LogWarning(FormatComponent(obj, component, memberName, filePath, lineNumber), component);
#else
        if (EnableRegister)
            OnLogMessageReceivedThreaded(FormatComponent(obj, component, memberName, filePath, lineNumber), string.Empty, LogType.Warning);
#endif     
    }

    /// <summary>
    /// Logs a message to the Unity console, optionally associating it with a specified component.
    /// </summary>
    /// <param name="obj">The object to log in the console.</param>
    /// <param name="component">Optional component to associate with the log message.</param>
    /// <param name="memberName"></param>
    /// <param name="filePath"></param>
    /// <param name="lineNumber"></param>
    public static void Log(object obj, Component component = null, [CallerMemberName]string memberName = "", [CallerFilePath] string filePath = "" , [CallerLineNumber] int lineNumber = 0)
    {
#if DEBUG
        Debug.Log(FormatComponent(obj, component, memberName, filePath, lineNumber), component);
#else
        if (EnableRegister)
            OnLogMessageReceivedThreaded(FormatComponent(obj, component, memberName, filePath, lineNumber), string.Empty, LogType.Log);
#endif  
    }

    /// <summary>
    /// Performs a log operation and associates the specified object with the console message.
    /// </summary>
    /// <param name="obj">The object to log.</param>
    /// <param name="component">An optional component to associate with the log message.</param>
    /// <param name="memberName"></param>
    /// <param name="filePath"></param>
    /// <param name="lineNumber"></param>
    public static void Log(this Component component, object obj, [CallerMemberName]string memberName = "", [CallerFilePath] string filePath = "" , [CallerLineNumber] int lineNumber = 0)
    {
        Log(obj, component, memberName, filePath, lineNumber);
    }

    /// <summary>
    /// Logs a message and Highlighted in the hierarchy the associated object in the Unity Editor.
    /// If a duration is specified, the object remains selected for the given period of time.
    /// </summary>
    /// <param name="obj">The object to be logged and selected.</param>
    /// <param name="component">Optional. The component to associate with the log message.</param>
    /// <param name="duration">The duration for which the object will remain selected, in seconds. Default is 2 seconds.</param>
    /// <param name="memberName"></param>
    /// <param name="filePath"></param>
    /// <param name="lineNumber"></param>
    /// <param name="background"></param>
    public static void LogAndMark(object obj, Component component = null, float duration = 2f, Color? background = null, Color? text = null,  [CallerMemberName]string memberName = "", [CallerFilePath] string filePath = "" , [CallerLineNumber] int lineNumber = 0)
    {
#if UNITY_EDITOR
        Highlight(obj, component, duration, background, text);
#endif
        Log(obj, component, memberName, filePath, lineNumber);
    }
    
    /// <summary>
    /// Logs the specified object, optionally formatted with the Component, and Highlighted in the hierarchy.
    /// If a duration is specified, the object remains selected for the given period of time.
    /// </summary>
    /// <param name="obj">The object to be logged.</param>
    /// <param name="component">The Component optionally associated with the log entry, which will be selected.</param>
    /// <param name="duration">The duration (in seconds) for which the object remains selected.</param>
    /// <param name="memberName"></param>
    /// <param name="filePath"></param>
    /// <param name="lineNumber"></param>
    public static void LogAndMark(this Component component, object obj, float duration = 2f, Color? background = null, Color? text = null, [CallerMemberName]string memberName = "", [CallerFilePath] string filePath = "" , [CallerLineNumber] int lineNumber = 0)
    {
        LogAndMark(obj, component, duration, background , text ,memberName, filePath, lineNumber);
    }


    /// <summary>
    /// Logs a message and selects the associated object in the Unity Editor.
    /// </summary>
    /// <param name="obj">The object to be logged and selected.</param>
    /// <param name="component">Optional. The component to associate with the log message.</param>
    /// <param name="duration">The duration for which the object will remain selected, in seconds. Default is 2 seconds.</param>
    /// <param name="memberName"></param>
    /// <param name="filePath"></param>
    /// <param name="lineNumber"></param>
    /// <param name="lineNumber"></param>
    /// <param name="background"></param>
    public static void LogAndSelect(object obj, Component component = null, float duration = 2f, Color? background = null, Color? text = null, [CallerMemberName]string memberName = "", [CallerFilePath] string filePath = "" , [CallerLineNumber] int lineNumber = 0)
    {
#if UNITY_EDITOR
        AndSelect(obj, component, duration, background , text);
#endif
        Log(obj, component, memberName, filePath, lineNumber);
    }

    /// <summary>
    /// Logs the specified object, optionally formatted with the Component, and selects the Component in the editor.
    /// If a duration is specified, the object remains selected for the given period of time.
    /// </summary>
    /// <param name="obj">The object to be logged.</param>
    /// <param name="component">The Component optionally associated with the log entry, which will be selected.</param>
    /// <param name="duration">The duration (in seconds) for which the object remains selected.</param>
    /// <param name="memberName"></param>
    /// <param name="filePath"></param>
    /// <param name="lineNumber"></param>
    /// <param name="lineNumber"></param>
    /// <param name="background"></param>
    public static void LogAndSelect(this Component component, object obj, float duration = 2f, Color? background = null, Color? text = null, [CallerMemberName]string memberName = "", [CallerFilePath] string filePath = "" , [CallerLineNumber] int lineNumber = 0)
    {
        LogAndSelect(obj, component, duration, background , text, memberName, filePath, lineNumber);
    }

    /// <summary>
    /// Logs a message and links the object to the console message,
    /// while also selecting and opening the object's properties in the editor.
    /// </summary>
    /// <param name="obj">The object to log and open properties for.</param>
    /// <param name="component">The optional component associated with the object.</param>
    /// <param name="duration">The duration for which details are highlighted and selected; default is 2 seconds.</param>
    /// /// <param name="memberName"></param>
    /// <param name="filePath"></param>
    /// <param name="lineNumber"></param>
    /// <param name="lineNumber"></param>
    /// <param name="background"></param>
    public static void LogAndOpen(object obj, Component component = null, float duration = 2f, Color? background = null, Color? text = null, [CallerMemberName]string memberName = "", [CallerFilePath] string filePath = "" , [CallerLineNumber] int lineNumber = 0)
    {
#if UNITY_EDITOR
        AndOpen(obj, component, duration, background , text);
#endif
        Log(obj, component, memberName, filePath, lineNumber);
    }

    /// <summary>
    /// Logs the given object and selects and opens the properties of the provided component or object.
    /// </summary>
    /// <param name="obj">The object to log and open.</param>
    /// <param name="component">The component to be selected and opened. Optional.</param>
    /// <param name="duration">The duration for which the properties remain open. Optional, default is 2 seconds.</param>
    /// <param name="memberName"></param>
    /// <param name="filePath"></param>
    /// <param name="lineNumber"></param>
    /// <param name="lineNumber"></param>
    /// <param name="background"></param>
    public static void LogAndOpen(this Component component, object obj, float duration = 2f, Color? background = null, Color? text = null, [CallerMemberName]string memberName = "", [CallerFilePath] string filePath = "" , [CallerLineNumber] int lineNumber = 0)
    {
        LogAndOpen(obj, component, duration, background , text , memberName, filePath, lineNumber);
    }

    /// <summary>
    /// Logs consecutive messages that are accumulated per frame.
    /// </summary>
    /// <param name="component">The component associated with the log message, or null if no component is specified.</param>
    /// <param name="obj">The object to be logged.</param>
    /// <param name="memberName"></param>
    /// <param name="filePath"></param>
    /// <param name="lineNumber"></param>
    public static void ConsecutiveLog(object obj, Component component = null, [CallerMemberName]string memberName = "", [CallerFilePath] string filePath = "" , [CallerLineNumber] int lineNumber = 0)
    {
#if UNITY_EDITOR
        if (!Chk)
            ExecuteInNextFrame += PrintCombinado;
            
        _debug.Add(FormatComponent(obj, component, memberName, filePath, lineNumber));
#else
        Log(obj, component, memberName, filePath, lineNumber);
#endif
    }

    /// <summary>
    /// Logs consecutivos, que se acumulan por frame
    /// </summary>
    /// <param name="obj">El objeto que se desea loguear</param>
    /// <param name="component">El componente opcional relacionado con el log</param>
    /// <param name="memberName"></param>
    /// <param name="filePath"></param>
    /// <param name="lineNumber"></param>
    public static void ConsecutiveLog(this Component component, object t, [CallerMemberName]string memberName = "", [CallerFilePath] string filePath = "" , [CallerLineNumber] int lineNumber = 0)
    {
        ConsecutiveLog(t, component, memberName, filePath, lineNumber);
    }

    /// <summary>
    /// Logs consecutive warnings that accumulate per frame.
    /// </summary>
    /// <param name="obj">The object to be logged as a warning.</param>
    /// <param name="component">The component context from which the log is generated, if any.</param>
    /// <param name="memberName"></param>
    /// <param name="filePath"></param>
    /// <param name="lineNumber"></param>
    public static void ConsecutiveWarning(object obj, Component component = null, [CallerMemberName]string memberName = "", [CallerFilePath] string filePath = "" , [CallerLineNumber] int lineNumber = 0)
    {
#if UNITY_EDITOR
        if (!Chk)
            ExecuteInNextFrame += PrintCombinado;
            
        _warning.Add($"<color=yellow>{FormatComponent(obj, component, memberName, filePath, lineNumber)}</color>");
#else
        Warning(component, obj, memberName, filePath, lineNumber);
#endif
    }

    /// <summary>
    /// Logs warning messages consecutively, which are accumulated per frame.
    /// </summary>
    /// <param name="obj">The object to be logged as a warning.</param>
    /// <param name="component">The Component associated with the logged message. Optional.</param>
    /// <param name="memberName"></param>
    /// <param name="filePath"></param>
    /// <param name="lineNumber"></param>
    public static void ConsecutiveWarning(this Component component, object obj, [CallerMemberName]string memberName = "", [CallerFilePath] string filePath = "" , [CallerLineNumber] int lineNumber = 0)
    {
        ConsecutiveWarning(obj, component, memberName, filePath, lineNumber);
    }


    /// <summary>
    /// Logs consecutive errors, which are accumulated per frame.
    /// </summary>
    /// <param name="obj">The object that represents the error message or details to be logged.</param>
    /// <param name="component">The component associated with the error, can be null.</param>
    /// <param name="memberName"></param>
    /// <param name="filePath"></param>
    /// <param name="lineNumber"></param>
    public static void ConsecutiveError(object obj, Component component = null, [CallerMemberName]string memberName = "", [CallerFilePath] string filePath = "" , [CallerLineNumber] int lineNumber = 0)
    {
#if UNITY_EDITOR
        if (!Chk)
            ExecuteInNextFrame += PrintCombinado;
            
        _error.Add($"<color=red>{FormatComponent(obj, component, memberName, filePath, lineNumber)}</color>");
#else
        Error(component, obj, memberName, filePath, lineNumber);
#endif
    }

    /// <summary>
    /// Logs consecutive errors, which are accumulated per frame.
    /// </summary>
    /// <param name="obj">The object that represents the error message or details to be logged.</param>
    /// <param name="component">The component associated with the error, can be null.</param>
    /// <param name="memberName"></param>
    /// <param name="filePath"></param>
    /// <param name="lineNumber"></param>
    public static void ConsecutiveError(this Component component, object t, [CallerMemberName]string memberName = "", [CallerFilePath] string filePath = "" , [CallerLineNumber] int lineNumber = 0)
    {
        ConsecutiveError(t, component, memberName, filePath, lineNumber);
    }

#endregion
}
