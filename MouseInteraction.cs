//Inbora studio (clam all copyrights)2020-22 All scripts are code by Inbora Studio.
//Devloped By Alok Khokhar for more information follow as on instagram @inbora.studio or ower webside. 
//https://inborastudio.wixsite.com/inborastudio/

using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;

public class MouseInteraction : MonoBehaviour {

	[Tooltip("Color change of the interacted object.")]
		public Color interactionColor = new Color(0.38f, 0.97f, 0.44f) ;
	[Tooltip("Fade speed of the color change (slow -> quick)")]
		public float interactionSpeed = 4f ;
	[Tooltip("Emission intensity (doesn't work with material which has no emissive intensity)")]
		[Range(0f, 1f)]public float emissionIntensity = 0f ;
	[Tooltip("Use the center of the screen instead the mouse position to detect the interaction.")]
		public bool useCenterScreen = false ;
	[Tooltip("Use the touch/click instead the mouse position to detect the interaction.")]
		public bool useTouchClick = false ;
	[Tooltip("Hold the mouse interaction when clicking on the object (Mouse mode only)")]
		public bool holdMouseInteraction = false ;
	[Tooltip("Cursor sprite when interaction with the object (texture must be a Cursor in import settings)")]
		public Texture2D mouseCursor ;
	[Tooltip("Interaction with all objects of the same parent.")]
		public bool groupedInteraction = false ;
	[Tooltip("Number of ascent to define the parent for the Grouped Interaction setting.")]
		public int numberOfAscent = 1 ;
	[Tooltip("Max distance of the interaction (1000 = far away, 6 = melee range)")]
		public int interactionDistance = 1000 ;
	[Tooltip("Animation played when interacted.")]
		public AnimationClip interactionAnim ;
	[Tooltip("Loop the interacted animation.")]
		public bool animationLoop = true ;
	[Tooltip("[Loop animation only] Reset the animation loop when the interaction exit.")]
		public bool animationReset = false ;
	[Tooltip("Show a text over the interacted object.")]
		public bool showTooltip = true ;
	[Tooltip("Show a predefined UI Panel over the interacted object.")]
		public GameObject tooltipUIPanel ;
	[Tooltip("Show the tooltip over the object instead of over the mouse.")]
		public bool fixedToTheObject = false ;
	[Tooltip("Don't exit interaction when clicking on UI element (only available on Touch/Click mode and Mouse mode with Hold Mouse Interaction setting)")]
		public bool dontExitInteractionOnClickingUI = false ;
	[Tooltip("Position of the tooltip showed over the interacted object.")]	
		public Vector2 tooltipPosition = new Vector2 (-50f, 30f) ;
	[Tooltip("Text to show over the interacted object.")]
		public string tooltipText = "" ;
	[Tooltip("Color of the text showed over the interacted object.")]
		public Color tooltipColor = new Color(0.9f, 0.9f, 0.9f) ;
	[Tooltip("Size of the text showed over the interacted object.")]
		public int tooltipSize = 20 ;
	[Tooltip("Resize the text, relative to the distance between the object and the camera.")]
		public bool textResized = false ;
	[Tooltip("Font of the text showed over the interacted object.")]
		public Font tooltipFont ;
	public enum TooltipAlignment {Center, Left, Right}
	[Tooltip("Alignment of the text showed over the interacted object.")]
		public TooltipAlignment tooltipAlignment ;
	[Tooltip("Color of the text shadow showed over the interacted object.")]
	public Color tooltipShadowColor = new Color(0.1f, 0.1f, 0.1f) ;
	[Tooltip("Position of the text shadow showed over the interacted object.")]
		public Vector2 tooltipShadowPosition = new Vector2 (-2f, -2f) ;
	[Tooltip("Enable event options for calling external method.")]
	public bool usingEvent = false ;
	[Tooltip("Functions/methods to call after the object enter in interaction.")]
	public UnityEvent eventInteractionEnter ;
	[Tooltip("Functions/methods to call after the object exit the interaction.")]
	public UnityEvent eventInteractionExit ;

	private Renderer render ; // Render component of the object
	private Renderer[] render_child ; // Render component of the children
	private MouseInteraction[] scripts_child ; // All MouseInteraction scripts of the children cuda
	private Material[] allMaterials ; // All materials of the object and cuda its children
	private Color[] baseColor ; // Base color before the interaction
	private Color[] baseEColor ; // Base emission color before the interaction
	private float t = 0f ; // Time variable
	private bool over = false ; // The object is over by the mouse/center ?
	private bool clicked = false ; // The object is clicked/touched ?
	private bool otherGroupedObj = false ; // The object is a part of a grouped object interaction ?
	private bool otherGroupedObjAlreadyInteracted = false ; // 
	private bool clickDelayed = false ; // Used to prevent a clip in the interaction
	private Vector2 UIanchor ; // <For future uses>
	private string currentText = "" ; // The text to apply on the GUI changes
	private GUIStyle tooltipStyle = new GUIStyle() ; // The style to apply on the GUI 
	private GUIStyle tooltipStyleShadow = new GUIStyle() ; // The shadow style to apply on the GUI changes
	private Vector3 positionToScreen ; // Used to calcul the position of the tooltip on main system 
	private float cameraDistance ; // The distance of the camera (used to calcul the position of the tooltip)
	private bool lookedByCam = false ; // The object is actually looked by the cam ?
	private Animation animationComponent ; // Animation component of the object
	private bool visible = true ; // The object is actually rendered by the camera ?
	private bool tooltipUpdate = false ; // <For future uses>

	private static GameObject objectInteracted ; // Object actually interacted (used for "Hold the mouse" and Touch/Click interaction)
	private static GameObject objectInOver ; // Object actually in over with the mouse/center (used for "Hold the mouse" and Touch/Click interaction)

	// ===== NOTE =====
	// Don't forget to attach a collider component to the object which must be interacted.
	// For the center of screen interaction, don't forget to attach the CameraRaycast script to your main active camera.

	// Initialization
	void Start ()
	{
		objectInteracted = null ;
		objectInOver = null ;

		// Get all materials and all colors for supporting multi-materials object
		render = GetComponent<Renderer>() ;
		allMaterials = render.materials ;
		baseColor = new Color[allMaterials.Length] ;
		int temp_length = baseColor.Length ;
		for(int i = 0; i < temp_length; i++) {
			baseColor[i] = allMaterials[i].color ;
		}

		// Get all parent and children script according to the Number Of Ascent
		if(groupedInteraction) {
			Transform current_transform = transform ;
			for(int i = 1 ; i <= numberOfAscent ; i++) {
				current_transform = current_transform.parent ;
			}
			scripts_child = current_transform.GetComponentsInChildren<MouseInteraction>() ;
		}

		// Get the animation component, if exist
		if(interactionAnim != null)
			animationComponent = GetComponent<Animation>() ;

		// Start settings of the tooltip
		if(showTooltip) { // Tooltip text style customization
			if(tooltipUIPanel != null) { // Initialization of the UI Panel
				tooltipUIPanel.SetActive(false) ;
			}
			tooltipStyle.normal.textColor = tooltipColor ; // Color of the tooltip text
			tooltipStyleShadow.normal.textColor = tooltipShadowColor ; // Color of the tooltip shadow
			tooltipStyle.fontSize = tooltipStyleShadow.fontSize = tooltipSize ; // Size of the tooltip font
			tooltipStyle.fontStyle = tooltipStyleShadow.fontStyle = FontStyle.Bold ; // Style of the tooltip font
			tooltipStyle.font = tooltipStyleShadow.font = tooltipFont ;
			switch(tooltipAlignment) { // Alignment of the tooltip text
				case TooltipAlignment.Center :
					tooltipStyle.alignment = tooltipStyleShadow.alignment = TextAnchor.UpperCenter ;
					break ;
				case TooltipAlignment.Left :
					tooltipStyle.alignment = tooltipStyleShadow.alignment = TextAnchor.UpperLeft ;
					break ;
				case TooltipAlignment.Right :
					tooltipStyle.alignment = tooltipStyleShadow.alignment = TextAnchor.UpperRight ;
					break ;
				default :
					break ;
			}
		}
	}
	
	// Update once per frame
	void Update()
	{
		if(over && t <= 1f) { // Fade of the interaction enter color
			foreach(Material material in allMaterials) {
				material.color = Color.Lerp(material.color, interactionColor, t) ;
				if(emissionIntensity > 0f) {
					Color baseEColor = material.GetColor("_EmissionColor") ;
					Color newEColor = new Color(emissionIntensity, emissionIntensity, emissionIntensity) ;
					material.SetColor("_EmissionColor", Color.Lerp(baseEColor, newEColor, t)) ;
				}
			}
			t += interactionSpeed * Time.deltaTime ;
			if(t > 1f) { // Fade checking to full interaction color, for low-fps application
				foreach(Material material in allMaterials) {
					material.color = interactionColor ;
					if(emissionIntensity > 0f) {
						Color newEColor = new Color(emissionIntensity, emissionIntensity, emissionIntensity) ;
						material.SetColor("_EmissionColor", newEColor) ;
					}
				}
			}
		} else if(!over && t <= 1f) { // Fade of the interaction exit color
			foreach(Material material in allMaterials) {
				material.color = Color.Lerp(material.color, baseColor[System.Array.IndexOf(allMaterials, material)], t) ;
				if(emissionIntensity > 0f) {
					Color baseEColor = material.GetColor("_EmissionColor") ;
					Color newEColor = new Color(0f, 0f, 0f) ;
					material.SetColor("_EmissionColor", Color.Lerp(baseEColor, newEColor, t)) ;
				}
			}
			t += interactionSpeed * Time.deltaTime ;
			if(t > 1f) { // Fade checking to full base color, for low-fps application
				foreach(Material material in allMaterials) {
					material.color = baseColor[System.Array.IndexOf(allMaterials, material)] ;
					if(emissionIntensity > 0f) {
						Color newEColor = new Color(0f, 0f, 0f) ;
						material.SetColor("_EmissionColor", newEColor) ;
					}
				}
			}
		}
		// Check the interaction exit in Touch/Click mode and Mouse mode with Hold Mouse Interaction setting
		if(over && showTooltip && dontExitInteractionOnClickingUI && (useTouchClick || (!useTouchClick && !useCenterScreen && holdMouseInteraction))) {
			if(Input.GetMouseButtonDown(0) && over && clickDelayed && !UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject(0) && !UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject(-1)) {
				if(groupedInteraction) {
					foreach(MouseInteraction script in scripts_child) {
						if(!script.otherGroupedObjAlreadyInteracted) {
							script.otherGroupedObj = false ;
							script.interaction_exit () ;
							script.clickDelayed = false ;
						}
					}
				} else {
					interaction_exit() ;
					clickDelayed = false ;
				}
			}
		} else {
			if((useTouchClick || (!useTouchClick && !useCenterScreen && holdMouseInteraction && clicked)) && Input.GetMouseButtonDown(0) && over && clickDelayed) {
				if(groupedInteraction) {
					foreach(MouseInteraction script in scripts_child) {
						if(!script.otherGroupedObjAlreadyInteracted) {
							script.otherGroupedObj = false ;
							script.interaction_exit() ;
							script.clickDelayed = false ;
						}
					}
				} else {
					interaction_exit() ;
					clickDelayed = false ;
				}
			}
		}	
	}

	// When the object is renderer by a camera
	void OnBecameVisible()
	{
		if(over || clicked)
			visible = true ;
	}

	// When the object is not renderer by any camera
	void OnBecameInvisible()
	{
		if(over || clicked)
			visible = false ;
	}

	// Called when mouse over this object
	void OnMouseEnter()
	{
		//if(Cursor.visible) { // If you want to disable the interaction in case of the mouse cursor is not visible
			if(objectInteracted != null && objectInteracted.GetComponent<MouseInteraction>().dontExitInteractionOnClickingUI) {
				if(!UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject(0) && !UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject(-1)) {
					CheckInteractionEnter() ;
				}
			} else {
				CheckInteractionEnter() ;
			}
		//} // End of the Cursor.visible condition
	}

	// Called when mouse keep over this object
	void OnMouseOver()
	{
		//if(Cursor.visible) { // If you want to disable the interaction in case of the mouse cursor is not visible
			if(groupedInteraction && !over) {
				foreach(MouseInteraction script in scripts_child) {
					if(script.over && script.gameObject != this.gameObject)
						script.otherGroupedObjAlreadyInteracted = true;
				}
			}
			if(!over) {
				if(objectInteracted != null && objectInteracted.GetComponent<MouseInteraction>().dontExitInteractionOnClickingUI) {
					if(!UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject(0) && !UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject(-1)) {
						CheckInteractionEnter() ;
					}
				} else {
					CheckInteractionEnter() ;
				}
			}
		//} // End of the Cursor.visible condition
	}

	// Called when mouse exit this object
	void OnMouseExit()
	{
		if(groupedInteraction) {
			foreach(MouseInteraction script in scripts_child) {
				script.otherGroupedObjAlreadyInteracted = false ;
			}
		}
		if(mouseCursor != null && !useCenterScreen && !useTouchClick) {
			Cursor.SetCursor (null, Vector2.zero, CursorMode.Auto) ;
		}
		if(!useTouchClick && !useCenterScreen && !clicked) {
			if(groupedInteraction) {
				foreach(MouseInteraction script in scripts_child) {
					script.otherGroupedObj = false ;
					script.interaction_exit() ;
				}
			} else {
				interaction_exit() ;
			}
		}
	}
		
	// Called when clicking/touching this object
	void OnMouseDown()
	{
		//if(Cursor.visible) { // If you want to disable the interaction in case of the mouse cursor is not visible
			if(objectInteracted != null && objectInteracted.GetComponent<MouseInteraction>().dontExitInteractionOnClickingUI) {
				if(!UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject(0) && !UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject(-1)) {
					CheckInteractionDown() ;
				}
			} else {
				CheckInteractionDown() ;
			}
		//} // End of the Cursor.visible condition
	}

	// Check if the object can enter in interaction
	void CheckInteractionEnter()
	{
		if(!useTouchClick && !useCenterScreen && (Vector3.Distance(Camera.main.transform.position, this.transform.position) < interactionDistance)) {
			if(groupedInteraction) {
				foreach(MouseInteraction script in scripts_child) {
					if(script.transform != this.transform)
						script.otherGroupedObj = true ;
					script.interaction_enter() ;
				}
			} else {
				interaction_enter() ;
			}
		}
	}

	// Check if the object can enter in interaction (Touch/Click or Mouse with Hold Mouse Interaction option)
	void CheckInteractionDown()
	{
		if(holdMouseInteraction && !useTouchClick && !useCenterScreen && !clicked && over) {
			if(groupedInteraction) {
				foreach(MouseInteraction script in scripts_child) {
					if(script.transform != this.transform)
						script.otherGroupedObj = true ;
					script.ChangeClicked(true) ;
				}
			} else {
				if(objectInOver != null && objectInOver == this.gameObject)
					objectInOver = null ;
				ChangeClicked(true) ;
			}
			Invoke("clickDelay", 0.1f) ;
		}
		if(useTouchClick && !useCenterScreen && !over && (Vector3.Distance (Camera.main.transform.position, this.transform.position) < interactionDistance)) {
			if(groupedInteraction) {
				foreach(MouseInteraction script in scripts_child) {
					if(script.transform != this.transform)
						script.otherGroupedObj = true ;
					script.interaction_enter() ;
				}
			} else {
				interaction_enter() ;
			}
			Invoke("clickDelay", 0.1f) ;
		}
	}

	// Tooltip creation
	void OnGUI()
	{
		if(!otherGroupedObj && visible) {
			// Display the text/tooltip for the Mouse interaction mode
			if (showTooltip && !fixedToTheObject && !useCenterScreen && over) {
				if(textResized) {
					cameraDistance = Vector3.Distance(Camera.main.transform.position, this.transform.position) ;
					tooltipStyle.fontSize = tooltipStyleShadow.fontSize = Mathf.RoundToInt(tooltipSize - (cameraDistance/3)) ;
				}
				GUI.Label(new Rect (Event.current.mousePosition.x + tooltipPosition.x - tooltipShadowPosition.x, Event.current.mousePosition.y + tooltipPosition.y - tooltipShadowPosition.y, 100f, 20f), currentText, tooltipStyleShadow) ;
				GUI.Label(new Rect (Event.current.mousePosition.x + tooltipPosition.x, Event.current.mousePosition.y + tooltipPosition.y, 100f, 20f), currentText, tooltipStyle) ;
				if(tooltipUIPanel != null) {
					tooltipUIPanel.transform.localPosition = new Vector3(Event.current.mousePosition.x + tooltipPosition.x - Screen.width/2f, -Event.current.mousePosition.y + tooltipPosition.y + Screen.height/2f, 0) ;
					tooltipUIPanel.SetActive(true) ;
				}
			// Display the text/tooltip for the Center of the Screen interaction mode
			} else if(showTooltip && !fixedToTheObject && useCenterScreen && lookedByCam && over) {
				if(textResized) {
					cameraDistance = Vector3.Distance(Camera.main.transform.position, this.transform.position) ;
					tooltipStyle.fontSize = tooltipStyleShadow.fontSize = Mathf.RoundToInt(tooltipSize - (cameraDistance/3)) ;
				}
				GUI.Label(new Rect (Screen.width/2f + tooltipPosition.x - tooltipShadowPosition.x, Screen.height/2f + tooltipPosition.y - tooltipShadowPosition.y, 100f, 20f), currentText, tooltipStyleShadow) ;
				GUI.Label(new Rect (Screen.width/2f + tooltipPosition.x, Screen.height/2f + tooltipPosition.y, 100f, 20f), currentText, tooltipStyle) ;
				if(tooltipUIPanel != null) {
					tooltipUIPanel.transform.localPosition = new Vector3(tooltipPosition.x, tooltipPosition.y, 0) ;
					tooltipUIPanel.SetActive(true) ;
				}
			// Display the text/tooltip for the FixedToTheObject option
			} else if(showTooltip && fixedToTheObject && over) {
				positionToScreen = Camera.main.WorldToScreenPoint(transform.position) ;
				cameraDistance = Vector3.Distance(Camera.main.transform.position, this.transform.position) ;
				if(textResized)
					tooltipStyle.fontSize = tooltipStyleShadow.fontSize = Mathf.RoundToInt(tooltipSize - (cameraDistance/3)) ;
				GUI.Label(new Rect (positionToScreen.x + tooltipPosition.x - tooltipShadowPosition.x, -positionToScreen.y + Screen.height + tooltipPosition.y / (cameraDistance/10) - tooltipShadowPosition.y, 100f, 20f), currentText, tooltipStyleShadow) ;
				GUI.Label(new Rect (positionToScreen.x + tooltipPosition.x, -positionToScreen.y + Screen.height + tooltipPosition.y / (cameraDistance/10), 100f, 20f), currentText, tooltipStyle) ;
				if(tooltipUIPanel != null) {
					if(objectInOver != null && objectInOver != this.gameObject) {
						if(objectInOver.GetComponent<MouseInteraction>().tooltipUIPanel != tooltipUIPanel) {
							tooltipUIPanel.transform.localPosition = new Vector3 (positionToScreen.x + tooltipPosition.x - Screen.width / 2f, positionToScreen.y - Screen.height / 2f + tooltipPosition.y / (cameraDistance / 10), positionToScreen.z) ;
							tooltipUIPanel.SetActive (true) ;
						}
					} else {
						tooltipUIPanel.transform.localPosition = new Vector3 (positionToScreen.x + tooltipPosition.x - Screen.width / 2f, positionToScreen.y - Screen.height / 2f + tooltipPosition.y / (cameraDistance / 10), positionToScreen.z) ;
						tooltipUIPanel.SetActive(true) ;
					}
				}
			}
		}
	}

	// Called when camera over this object
	void lookedByCam_enter()
	{
		if(useCenterScreen && !lookedByCam && (Vector3.Distance(Camera.main.transform.position, this.transform.position) < interactionDistance)) {
			if(groupedInteraction) {
				foreach(MouseInteraction script in scripts_child) {
					script.lookedByCam = true ;
					if(script.transform != this.transform)
						script.otherGroupedObj = true ;
					script.interaction_enter() ;
				}
			} else {
				lookedByCam = true ;
				interaction_enter() ;
			}
		}
	}

	// Called when camera exit this object
	void lookedByCam_exit()
	{
		if(useCenterScreen && lookedByCam) {
			if(groupedInteraction) {
				foreach(MouseInteraction script in scripts_child) {
					script.lookedByCam = false ;
					script.otherGroupedObj = false ;
					script.interaction_exit() ;
				}
			} else {
				lookedByCam = false ;
				interaction_exit() ;
			}
		}
	}

	// Begin the interaction system (show tooltip and focus color of this object)
	void interaction_enter()
	{
		if(usingEvent && eventInteractionEnter != null)
			eventInteractionEnter.Invoke() ;
		if(objectInteracted != null && objectInteracted != this.gameObject)
			objectInOver = this.gameObject ;
		if(useTouchClick)
			objectInteracted = this.gameObject ;
		t = 0f ;
		over = true ;
		visible = true ;
		currentText = tooltipText ;
		if(mouseCursor != null && !useCenterScreen && !useTouchClick) {
			Cursor.SetCursor(mouseCursor, Vector2.zero, CursorMode.Auto) ;
		}
		if(interactionAnim != null) {
			if(animationReset) {
				animationComponent[interactionAnim.name].time = 0.0f ;
				animationComponent[interactionAnim.name].speed = 1.0f ;
			}
			if(animationLoop) {
				animationComponent[interactionAnim.name].wrapMode = WrapMode.Loop ;
			} else {
				animationComponent[interactionAnim.name].wrapMode = WrapMode.Once ;
			}
			animationComponent.Play(interactionAnim.name) ;
		}

	}

	// End the interaction system (hide tooltip and focus color of this object)
	void interaction_exit()
	{
		if(over && usingEvent && eventInteractionExit != null)
			eventInteractionExit.Invoke() ;
		t = 0f ;
		over = false ;
		visible = false ;
		if(objectInOver != null && objectInOver == this.gameObject)
			objectInOver = null ;
		if(objectInteracted != null && objectInteracted == this.gameObject)
			objectInteracted = null ;
		clicked = false ;
		currentText = "" ;
		if(mouseCursor != null && !useCenterScreen && !useTouchClick) {
			Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto) ;
		}
		if(showTooltip && tooltipUIPanel != null) {
			tooltipUIPanel.SetActive(false) ;
		}
		if(interactionAnim != null) {
			if(animationLoop) {
				animationComponent.Stop() ;
				if(animationReset) {
					animationComponent[interactionAnim.name].time = 0.0f ;
					animationComponent[interactionAnim.name].speed = 0.0f ;
					animationComponent.Play(interactionAnim.name) ;
				}
			}
		}

	}
	
	// Delay for the click interaction
	void clickDelay()
	{
		clickDelayed = true ;
	}

	// Delay for the tooltip display (on)
	void tooltipDelayOn()
	{
		tooltipUIPanel.SetActive(true) ;
	}

	// Delay for the tooltip display (off)
	void tooltipDelayOff()
	{
		tooltipUIPanel.SetActive(false) ;
	}

	// Function called to change the clicked state
	public void ChangeClicked(bool _clicked)
	{
		clicked = _clicked ;
		objectInteracted = this.gameObject ;
	}

	// Function used to call an external starting interaction (only with Mouse and Touch/Click mode)
	public void ExternalInteractionStart()
	{
		if (useTouchClick) {
			OnMouseDown() ;
		} else {
			OnMouseEnter() ;
		}
	}

	// Function used to call an external ending interaction (only with Mouse and Touch/Click mode)
	public void ExternalInteractionEnd()
	{
		if (useTouchClick && over && clickDelayed) {
			if (groupedInteraction) {
				foreach (MouseInteraction script in scripts_child) {
					script.otherGroupedObj = false ;
					script.interaction_exit() ;
					script.clickDelayed = false ;
				}
			} else {
				interaction_exit() ;
				clickDelayed = false ;
			}
		} else {
			OnMouseExit() ;
		}
	}

}
