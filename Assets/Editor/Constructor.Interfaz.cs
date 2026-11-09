using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

//HUD de los niveles, menú de inicio y pantalla de victoria
public static partial class ConstructorProyecto {

	static readonly Color AMARILLO = new Color(1f, 0.85f, 0.25f);
	static readonly Color OSCURO = new Color(0.09f, 0.05f, 0.03f, 1f);

	static RectTransform Rect(GameObject go, Vector2 anclaMin, Vector2 anclaMax, Vector2 pivote, Vector2 pos, Vector2 tam) {
		var rt = go.GetComponent<RectTransform>();
		rt.anchorMin = anclaMin;
		rt.anchorMax = anclaMax;
		rt.pivot = pivote;
		rt.anchoredPosition = pos;
		rt.sizeDelta = tam;
		return rt;
	}

	static GameObject UI(string nombre, Transform padre) {
		var go = new GameObject(nombre, typeof(RectTransform));
		go.transform.SetParent(padre, false);
		return go;
	}

	static Text Texto(Transform padre, string nombre, string contenido, int tamano, TextAnchor alineacion,
		Vector2 ancla, Vector2 pos, Vector2 tam, Color? color = null, bool contorno = true) {
		var go = UI(nombre, padre);
		Rect(go, ancla, ancla, ancla, pos, tam);
		var t = go.AddComponent<Text>();
		t.font = fuente;
		t.text = contenido;
		t.fontSize = tamano;
		t.fontStyle = FontStyle.Bold;
		t.alignment = alineacion;
		t.color = color ?? Color.white;
		t.horizontalOverflow = HorizontalWrapMode.Wrap;
		t.verticalOverflow = VerticalWrapMode.Overflow;
		t.raycastTarget = false;
		t.lineSpacing = 1.05f;
		if (contorno) {
			var o = go.AddComponent<Outline>();
			o.effectColor = OSCURO;
			o.effectDistance = new Vector2(3f, -3f);
			var s = go.AddComponent<Shadow>();
			s.effectColor = new Color(0f, 0f, 0f, 0.5f);
			s.effectDistance = new Vector2(4f, -5f);
		}
		return t;
	}

	static Image Imagen(Transform padre, string nombre, Sprite s, Vector2 ancla, Vector2 pos, Vector2 tam, Color? color = null) {
		var go = UI(nombre, padre);
		Rect(go, ancla, ancla, ancla, pos, tam);
		var img = go.AddComponent<Image>();
		img.sprite = s;
		img.color = color ?? Color.white;
		img.raycastTarget = false;
		if (s != null && s.border != Vector4.zero) img.type = Image.Type.Sliced;
		return img;
	}

	static Image Panel(Transform padre, string nombre, Vector2 pos, Vector2 tam) {
		var img = Imagen(padre, nombre, sPanel, new Vector2(0.5f, 0.5f), pos, tam);
		img.pixelsPerUnitMultiplier = 2f;
		return img;
	}

	static Canvas CanvasPantalla(string nombre, int orden) {
		var go = new GameObject(nombre, typeof(RectTransform));
		var canvas = go.AddComponent<Canvas>();
		canvas.renderMode = RenderMode.ScreenSpaceOverlay;
		canvas.sortingOrder = orden;
		var scaler = go.AddComponent<CanvasScaler>();
		scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
		scaler.referenceResolution = new Vector2(1920, 1080);
		scaler.matchWidthOrHeight = 0.5f;
		go.AddComponent<GraphicRaycaster>();
		return canvas;
	}

	static EventSystem SistemaEventos() {
		var es = new GameObject("EventSystem");
		es.AddComponent<EventSystem>();
		es.AddComponent<StandaloneInputModule>();
		return es.GetComponent<EventSystem>();
	}

	static Button Boton(Transform padre, string texto, Vector2 pos, Color color, UnityEngine.Events.UnityAction accion) {
		var go = UI("Boton_" + texto, padre);
		Rect(go, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), pos, new Vector2(460f, 96f));
		var img = go.AddComponent<Image>();
		img.sprite = sBoton;
		img.type = Image.Type.Sliced;
		img.pixelsPerUnitMultiplier = 2f;
		img.color = color;
		var b = go.AddComponent<Button>();
		var colores = b.colors;
		colores.highlightedColor = new Color(1.15f, 1.15f, 1.15f);
		colores.pressedColor = new Color(0.75f, 0.75f, 0.75f);
		colores.selectedColor = new Color(1.1f, 1.1f, 1.1f);
		b.colors = colores;
		UnityEventTools.AddPersistentListener(b.onClick, accion);
		Texto(go.transform, "Texto", texto, 44, TextAnchor.MiddleCenter, new Vector2(0.5f, 0.5f), new Vector2(0f, 4f), new Vector2(440f, 90f));
		return b;
	}

	static void NombreYMatricula(Transform canvas) {
		Texto(canvas, "NombreMatricula", NOMBRE + "\nMatrícula " + MATRICULA, 24, TextAnchor.UpperRight,
			new Vector2(1f, 1f), new Vector2(-18f, -14f), new Vector2(350f, 70f), new Color(1f, 1f, 1f, 0.9f));
	}

	// ------------------------------------------------ HUD de los niveles

	static void CrearHUD(Nivel nivel, bool mostrarControles) {

		var canvas = CanvasPantalla("HUD", 10).transform;

		var barra = Imagen(canvas, "Barra", sBlanco, new Vector2(0.5f, 1f), Vector2.zero, new Vector2(1920f, 130f), new Color(0f, 0f, 0f, 0.35f));
		barra.rectTransform.anchorMin = new Vector2(0f, 1f);
		barra.rectTransform.anchorMax = new Vector2(1f, 1f);
		barra.rectTransform.pivot = new Vector2(0.5f, 1f);
		barra.rectTransform.sizeDelta = new Vector2(0f, 120f);

		var izq = new Vector2(0f, 1f);
		nivel.corazones = new Image[Juego.VIDAS_MAX];
		for (int i = 0; i < Juego.VIDAS_MAX; i++) {
			nivel.corazones[i] = Imagen(canvas, "Corazon" + i, sCorazon, izq, new Vector2(30f + i * 58f, -30f), new Vector2(56f, 56f));
			nivel.corazones[i].rectTransform.pivot = new Vector2(0f, 1f);
		}
		Texto(canvas, "TituloVidas", "VIDAS", 22, TextAnchor.UpperLeft, izq, new Vector2(34f, -84f), new Vector2(200f, 30f), AMARILLO);
		nivel.txtVidas = Texto(canvas, "Vidas", "x3", 22, TextAnchor.UpperLeft, izq, new Vector2(120f, -84f), new Vector2(100f, 30f));

		var icono = Imagen(canvas, "IconoLarimar", sLarimar, izq, new Vector2(360f, -30f), new Vector2(56f, 56f));
		icono.rectTransform.pivot = new Vector2(0f, 1f);
		nivel.txtLarimares = Texto(canvas, "Larimares", "x00", 44, TextAnchor.UpperLeft, izq, new Vector2(425f, -30f), new Vector2(200f, 60f));
		Texto(canvas, "TituloLarimares", "LARIMARES", 22, TextAnchor.UpperLeft, izq, new Vector2(360f, -84f), new Vector2(240f, 30f), AMARILLO);

		var centro = new Vector2(0.5f, 1f);
		nivel.txtNivel = Texto(canvas, "Nivel", "NIVEL 1\nPLAYA", 30, TextAnchor.UpperCenter, centro, new Vector2(-90f, -16f), new Vector2(500f, 100f));
		nivel.txtPuntos = Texto(canvas, "Puntos", "PUNTOS\n000000", 30, TextAnchor.UpperCenter, centro, new Vector2(270f, -16f), new Vector2(240f, 100f));
		nivel.txtTiempo = Texto(canvas, "Tiempo", "TIEMPO\n200", 30, TextAnchor.UpperCenter, centro, new Vector2(490f, -16f), new Vector2(200f, 100f));
		NombreYMatricula(canvas);

		if (mostrarControles) {
			Texto(canvas, "Controles", "← →  mover     ESPACIO / ↑  saltar     SHIFT  correr     P  pausa",
				24, TextAnchor.LowerCenter, new Vector2(0.5f, 0f), new Vector2(0f, 60f), new Vector2(1600f, 40f), new Color(1f, 1f, 1f, 0.85f));
		}

		//Mensaje central (inicio del nivel, muerte, nivel completado)
		var panel = Panel(canvas, "PanelMensaje", new Vector2(0f, 60f), new Vector2(1250f, 330f));
		nivel.panelMensaje = panel.gameObject;
		nivel.txtMensaje = Texto(panel.transform, "Titulo", "NIVEL 1", 84, TextAnchor.MiddleCenter, new Vector2(0.5f, 0.5f), new Vector2(0f, 70f), new Vector2(1200f, 110f), AMARILLO);
		nivel.txtSubMensaje = Texto(panel.transform, "Subtitulo", "", 38, TextAnchor.UpperCenter, new Vector2(0.5f, 0.5f), new Vector2(0f, -30f), new Vector2(1150f, 200f));

		//Pausa
		var pausa = Imagen(canvas, "PanelPausa", sBlanco, new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero, new Color(0f, 0f, 0.05f, 0.65f));
		pausa.rectTransform.anchorMin = Vector2.zero;
		pausa.rectTransform.anchorMax = Vector2.one;
		pausa.raycastTarget = true;
		Texto(pausa.transform, "Titulo", "PAUSA", 120, TextAnchor.MiddleCenter, new Vector2(0.5f, 0.5f), new Vector2(0f, 80f), new Vector2(900f, 160f), AMARILLO);
		Texto(pausa.transform, "Ayuda", "P o ESC: continuar\nM: volver al menú principal", 42, TextAnchor.MiddleCenter, new Vector2(0.5f, 0.5f), new Vector2(0f, -70f), new Vector2(1200f, 140f));
		nivel.panelPausa = pausa.gameObject;

		//Barra de vida del jefe
		var jefe = UI("PanelJefe", canvas);
		Rect(jefe, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 40f), new Vector2(900f, 110f));
		Texto(jefe.transform, "Nombre", "EL GALIPOTE", 38, TextAnchor.MiddleCenter, new Vector2(0.5f, 1f), new Vector2(0f, -22f), new Vector2(800f, 50f), new Color(1f, 0.5f, 0.4f));
		Imagen(jefe.transform, "Fondo", sBlanco, new Vector2(0.5f, 0f), new Vector2(0f, 10f), new Vector2(820f, 44f), OSCURO).rectTransform.pivot = new Vector2(0.5f, 0f);
		var relleno = Imagen(jefe.transform, "Relleno", sBlanco, new Vector2(0.5f, 0f), new Vector2(0f, 16f), new Vector2(808f, 32f), new Color(0.85f, 0.15f, 0.2f));
		relleno.rectTransform.pivot = new Vector2(0.5f, 0f);
		relleno.type = Image.Type.Filled;
		relleno.fillMethod = Image.FillMethod.Horizontal;
		relleno.fillOrigin = 0;
		nivel.panelJefe = jefe;
		nivel.barraJefe = relleno;
	}

	// ------------------------------------------------ Escenario decorativo del menú y la victoria

	static void EscenarioPlaya(int ancho, bool noche) {
		var tema = temas[noche ? 2 : 0];
		CrearCamara(tema, ancho, null, !noche, noche);
		var grid = Objeto("Grid");
		grid.AddComponent<Grid>();
		var tmGo = Objeto("Suelo", grid.transform, CAPA_SUELO);
		var tm = tmGo.AddComponent<Tilemap>();
		tmGo.AddComponent<TilemapRenderer>();
		var sup = CrearTile(tema.nombre + "_Superficie", tema.superficie);
		var inte = CrearTile(tema.nombre + "_Interior", tema.interior);
		for (int x = -2; x < ancho + 2; x++) {
			tm.SetTile(new Vector3Int(x, 0, 0), inte);
			tm.SetTile(new Vector3Int(x, 1, 0), sup);
		}
		if (!noche) {
			Render(Objeto("Palmera"), sPalmera, -6).transform.position = new Vector3(3f, 2f, 0f);
			Render(Objeto("Palmera"), sPalmera, -6).transform.position = new Vector3(24f, 2f, 0f);
			Render(Objeto("Casita"), sCasita1, -7).transform.position = new Vector3(7.5f, 2f, 0f);
			Render(Objeto("Casita"), sCasita2, -7).transform.position = new Vector3(20f, 2f, 0f);
			Render(Objeto("Arbusto"), sArbusto, -4).transform.position = new Vector3(12f, 2f, 0f);
		}
	}

	static Image ImagenAnimada(Transform padre, string nombre, Sprite[] frames, Vector2 pos, Vector2 tam, float fps, bool espejo = false) {
		var img = Imagen(padre, nombre, frames[0], new Vector2(0.5f, 0.5f), pos, tam);
		img.preserveAspect = true;
		if (espejo) img.rectTransform.localScale = new Vector3(-1f, 1f, 1f);
		var a = img.gameObject.AddComponent<AnimadorSprite>();
		a.frames = frames;
		a.fps = fps;
		return img;
	}

	// ------------------------------------------------ Menú de inicio

	static void CrearMenu() {
		EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
		EscenarioPlaya(27, false);
		var eventos = SistemaEventos();

		var canvas = CanvasPantalla("Canvas", 10).transform;
		var menu = canvas.gameObject.AddComponent<MenuPrincipal>();
		var c = new Vector2(0.5f, 0.5f);

		var principal = UI("PanelPrincipal", canvas);
		Rect(principal, Vector2.zero, Vector2.one, c, Vector2.zero, Vector2.zero);
		var principalT = principal.transform;
		Texto(principalT, "Titulo", "CIGUÍN", 190, TextAnchor.MiddleCenter, new Vector2(0.5f, 1f), new Vector2(0f, -110f), new Vector2(1400f, 230f), AMARILLO);
		Texto(principalT, "Subtitulo", "y la Leyenda del Galipote", 64, TextAnchor.MiddleCenter, new Vector2(0.5f, 1f), new Vector2(0f, -330f), new Vector2(1400f, 90f));
		NombreYMatricula(canvas);
		Texto(canvas, "Creditos", "Programación de Videojuegos · Juego de plataformas 2D · " + NOMBRE, 24, TextAnchor.LowerCenter,
			new Vector2(0.5f, 0f), new Vector2(0f, 16f), new Vector2(1800f, 40f), new Color(1f, 1f, 1f, 0.85f));

		//Panel principal
		ImagenAnimada(principal.transform, "Ciguin", sJugadorCaminar, new Vector2(-620f, -150f), new Vector2(256f, 256f), 8f);
		ImagenAnimada(principal.transform, "Galipote", sJefe, new Vector2(620f, -130f), new Vector2(300f, 300f), 4f);
		var jugar = Boton(principal.transform, "JUGAR", new Vector2(0f, -10f), new Color(0.3f, 0.8f, 0.35f), menu.Jugar);
		Boton(principal.transform, "CÓMO JUGAR", new Vector2(0f, -125f), new Color(0.3f, 0.65f, 0.95f), menu.Controles);
		Boton(principal.transform, "HISTORIA", new Vector2(0f, -240f), new Color(0.95f, 0.6f, 0.25f), menu.Historia);
		menu.txtParpadeo = Texto(principal.transform, "Parpadeo", "Presiona ENTER para empezar", 34, TextAnchor.MiddleCenter, c, new Vector2(0f, -340f), new Vector2(900f, 50f), AMARILLO);
		menu.txtMejorPuntaje = Texto(principal.transform, "MejorPuntaje", "MEJOR PUNTAJE: 000000", 30, TextAnchor.MiddleCenter, c, new Vector2(0f, -395f), new Vector2(900f, 50f));
		menu.panelPrincipal = principal;
		eventos.firstSelectedGameObject = jugar.gameObject;

		//Panel cómo jugar: controles, objetos y enemigos con sus dibujos
		var controles = Panel(canvas, "PanelControles", new Vector2(0f, 0f), new Vector2(1700f, 900f)).gameObject;
		var tc = controles.transform;
		Texto(tc, "Titulo", "CÓMO JUGAR", 56, TextAnchor.MiddleCenter, new Vector2(0.5f, 1f), new Vector2(0f, -50f), new Vector2(1000f, 70f), AMARILLO);
		Texto(tc, "Controles",
			"← → o A D: moverse\nESPACIO, ↑, W o Z: saltar (mantén para saltar más alto)\nSHIFT o X: correr\nP o ESC: pausa\n\n" +
			"Salta encima de los enemigos para vencerlos y golpea los bloques desde abajo. " +
			"Llega a la bandera antes de que se acabe el tiempo. Si pierdes todas las vidas o se acaba el tiempo, vuelves al Nivel 1.",
			28, TextAnchor.UpperLeft, new Vector2(0f, 1f), new Vector2(60f, -100f), new Vector2(760f, 560f), Color.white);
		var items = new (Sprite s, string t)[] {
			(sLarimar, "Larimar: +100 puntos. 50 = 1 vida"),
			(sMango, "Mango: +1 vida"),
			(sSorpresa[0], "Bloque sorpresa"),
			(sCangrejo[0], "Cangrejo: se puede aplastar"),
			(sMurcielago[0], "Murciélago: se lanza en picada"),
			(sCactus[0], "Cactus: lanza espinas, ¡no lo pises!"),
			(sCoco, "Coco / estalactita: caen al pasar"),
			(sPuas, "Púas y plataformas que se caen"),
			(sJefe[0], "El Galipote: jefe del nivel 3"),
		};
		for (int i = 0; i < items.Length; i++) {
			var p = new Vector2(870f, -110f - i * 70f);
			var im = Imagen(tc, "Icono" + i, items[i].s, new Vector2(0f, 1f), p, new Vector2(54f, 54f));
			im.preserveAspect = true;
			im.rectTransform.pivot = new Vector2(0f, 1f);
			Texto(tc, "Item" + i, items[i].t, 28, TextAnchor.MiddleLeft, new Vector2(0f, 1f), p + new Vector2(70f, -27f), new Vector2(720f, 54f)).rectTransform.pivot = new Vector2(0f, 0.5f);
		}
		Texto(tc, "Volver", "ESC: volver", 26, TextAnchor.LowerRight, new Vector2(1f, 0f), new Vector2(-40f, 30f), new Vector2(400f, 40f), AMARILLO).rectTransform.pivot = new Vector2(1f, 0f);
		menu.panelControles = controles;

		//Panel historia
		var historia = Panel(canvas, "PanelHistoria", new Vector2(0f, 0f), new Vector2(1500f, 760f)).gameObject;
		Texto(historia.transform, "Titulo", "LA HISTORIA", 56, TextAnchor.MiddleCenter, new Vector2(0.5f, 1f), new Vector2(0f, -50f), new Vector2(1000f, 70f), AMARILLO);
		Texto(historia.transform, "Texto",
			"En la isla vivía Ciguín, una pequeña cigua palmera con su sombrero de guano. " +
			"Una noche el Galipote, el ser del folclor que cambia de forma, se robó todos los larimares de la isla " +
			"y llenó la playa, la cueva y la loma de criaturas que le obedecen.\n\n" +
			"Ciguín debe cruzar la Playa de Samaná, atravesar la Cueva de las Maravillas y subir a la Loma del Galipote " +
			"para enfrentarlo y devolverle la paz a su gente.",
			32, TextAnchor.UpperLeft, new Vector2(0.5f, 1f), new Vector2(0f, -110f), new Vector2(1360f, 520f)).rectTransform.pivot = new Vector2(0.5f, 1f);
		Texto(historia.transform, "Volver", "ESC: volver", 26, TextAnchor.LowerRight, new Vector2(1f, 0f), new Vector2(-40f, 30f), new Vector2(400f, 40f), AMARILLO).rectTransform.pivot = new Vector2(1f, 0f);
		menu.panelHistoria = historia;

		//Botones "volver" con el ratón
		foreach (var panel in new[] { controles, historia }) {
			var b = Boton(panel.transform, "VOLVER", Vector2.zero, new Color(0.85f, 0.35f, 0.3f), menu.Volver);
			var rt = b.GetComponent<RectTransform>();
			rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0.5f, 0f);
			rt.anchoredPosition = new Vector2(0f, 25f);
			rt.sizeDelta = new Vector2(300f, 80f);
		}

		GuardarEscena("Menu");
	}

	// ------------------------------------------------ Victoria

	static void CrearVictoria() {
		EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
		EscenarioPlaya(27, false);
		var eventos = SistemaEventos();

		var canvas = CanvasPantalla("Canvas", 10).transform;
		var v = canvas.gameObject.AddComponent<PantallaVictoria>();
		var c = new Vector2(0.5f, 0.5f);
		Texto(canvas, "Titulo", "¡VICTORIA!", 170, TextAnchor.MiddleCenter, new Vector2(0.5f, 1f), new Vector2(0f, -170f), new Vector2(1400f, 210f), AMARILLO);
		Texto(canvas, "Mensaje", "Ciguín derrotó al Galipote, recuperó los larimares\ny la paz volvió a la isla.", 48, TextAnchor.MiddleCenter,
			new Vector2(0.5f, 1f), new Vector2(0f, -340f), new Vector2(1500f, 140f));
		var panel = Panel(canvas, "PanelPuntos", new Vector2(0f, -60f), new Vector2(820f, 260f));
		v.txtPuntos = Texto(panel.transform, "Puntos", "PUNTOS: 000000", 44, TextAnchor.MiddleCenter, c, Vector2.zero, new Vector2(780f, 230f));
		ImagenAnimada(canvas, "Ciguin", new[] { sJugadorCelebrar, sJugadorQuieto[0] }, new Vector2(-640f, -170f), new Vector2(256f, 256f), 3f);
		ImagenAnimada(canvas, "Mango", new[] { sMango }, new Vector2(640f, -170f), new Vector2(180f, 180f), 1f);
		var b = Boton(canvas, "MENÚ", new Vector2(0f, -280f), new Color(0.3f, 0.8f, 0.35f), v.Volver);
		v.txtParpadeo = Texto(canvas, "Parpadeo", "Presiona ENTER para volver al menú", 34, TextAnchor.MiddleCenter, c, new Vector2(0f, -385f), new Vector2(1000f, 50f), AMARILLO);
		eventos.firstSelectedGameObject = b.gameObject;
		NombreYMatricula(canvas);
		GuardarEscena("Victoria");
	}
}
