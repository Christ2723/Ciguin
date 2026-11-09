using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Tilemaps;

//Construye automáticamente todo el juego "Ciguín y la Leyenda del Galipote":
//sprites, sonidos, música, prefabs, los 3 niveles, el menú y la pantalla de victoria.
//Se ejecuta desde el menú "Ciguin" de Unity o por línea de comandos (-executeMethod).
public static partial class ConstructorProyecto {

	const string NOMBRE = "Christopher Payero Estrella";
	const string MATRICULA = "1-21-0909";
	const string TITULO = "Ciguín y la Leyenda del Galipote";

	const int CAPA_SUELO = 8;
	const int CAPA_JUGADOR = 9;
	const int CAPA_ENEMIGOS = 10;
	const int CAPA_GOLPE = 11;
	const int CAPA_OBJETOS = 12;
	const int CAPA_RECOGIBLE = 13;

	static readonly string[] escenas = { "Menu", "Nivel1", "Nivel2", "Nivel3", "Victoria" };

	static Font fuente;
	static PhysicsMaterial2D sinFriccion;
	static Juego juegoPrefab;

	[MenuItem("Ciguin/Construir todo")]
	public static void ConstruirTodo() {

		foreach (var carpeta in new[] { "Scenes", "Prefabs", "Sprites", "Audio", "Tiles", "Resources", "Materiales" }) {
			if (!AssetDatabase.IsValidFolder("Assets/" + carpeta)) AssetDatabase.CreateFolder("Assets", carpeta);
		}
		fuente = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

		ConfigurarCapas();
		ConfigurarEjes();
		CrearArte();
		CrearJuegoPrefab();
		CrearPrefabs();

		CrearMenu();
		CrearNivel1();
		CrearNivel2();
		CrearNivel3();
		CrearVictoria();

		EditorBuildSettings.scenes = escenas.Select(e => new EditorBuildSettingsScene("Assets/Scenes/" + e + ".unity", true)).ToArray();

		PlayerSettings.companyName = NOMBRE;
		PlayerSettings.productName = "Ciguin";
		PlayerSettings.defaultWebScreenWidth = 1280;
		PlayerSettings.defaultWebScreenHeight = 720;
		PlayerSettings.runInBackground = true;
		PlayerSettings.WebGL.compressionFormat = WebGLCompressionFormat.Disabled;
		PlayerSettings.WebGL.decompressionFallback = false;
		PlayerSettings.SplashScreen.show = false;

		AssetDatabase.SaveAssets();
		Debug.Log("ConstructorProyecto: todo construido");
	}

	[MenuItem("Ciguin/Compilar WebGL")]
	public static void CompilarWebGL() => Compilar(BuildTarget.WebGL, "Builds/WebGL");

	[MenuItem("Ciguin/Compilar Windows")]
	public static void CompilarWindows() => Compilar(BuildTarget.StandaloneWindows64, "Builds/Windows/Ciguin.exe");

	static void Compilar(BuildTarget destino, string ruta) {
		var opciones = new BuildPlayerOptions {
			scenes = escenas.Select(e => "Assets/Scenes/" + e + ".unity").ToArray(),
			locationPathName = ruta,
			target = destino,
			options = BuildOptions.None
		};
		var reporte = BuildPipeline.BuildPlayer(opciones);
		Debug.Log("ConstructorProyecto: build " + destino + " " + reporte.summary.result);
		if (reporte.summary.result != UnityEditor.Build.Reporting.BuildResult.Succeeded && Application.isBatchMode) EditorApplication.Exit(1);
	}

	// ------------------------------------------------ Configuración del proyecto

	static void ConfigurarCapas() {
		var tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
		var capas = tagManager.FindProperty("layers");
		void Capa(int i, string n) => capas.GetArrayElementAtIndex(i).stringValue = n;
		Capa(CAPA_SUELO, "Suelo");
		Capa(CAPA_JUGADOR, "Jugador");
		Capa(CAPA_ENEMIGOS, "Enemigos");
		Capa(CAPA_GOLPE, "Golpe");
		Capa(CAPA_OBJETOS, "Objetos");
		Capa(CAPA_RECOGIBLE, "Recogible");
		tagManager.ApplyModifiedProperties();
		AssetDatabase.SaveAssets();
		FisicaCapas.Configurar();

		sinFriccion = new PhysicsMaterial2D("SinFriccion") { friction = 0f, bounciness = 0f };
		string ruta = "Assets/Materiales/SinFriccion.physicsMaterial2D";
		AssetDatabase.DeleteAsset(ruta);
		AssetDatabase.CreateAsset(sinFriccion, ruta);
	}

	static void ConfigurarEjes() {
		var inputManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/InputManager.asset")[0]);
		var ejes = inputManager.FindProperty("m_Axes");
		ejes.ClearArray();
		void Eje(string nombre, string negativo, string positivo, string altNegativo = "", string altPositivo = "") {
			ejes.arraySize++;
			var e = ejes.GetArrayElementAtIndex(ejes.arraySize - 1);
			e.FindPropertyRelative("m_Name").stringValue = nombre;
			e.FindPropertyRelative("descriptiveName").stringValue = "";
			e.FindPropertyRelative("descriptiveNegativeName").stringValue = "";
			e.FindPropertyRelative("negativeButton").stringValue = negativo;
			e.FindPropertyRelative("positiveButton").stringValue = positivo;
			e.FindPropertyRelative("altNegativeButton").stringValue = altNegativo;
			e.FindPropertyRelative("altPositiveButton").stringValue = altPositivo;
			e.FindPropertyRelative("gravity").floatValue = 1000f;
			e.FindPropertyRelative("dead").floatValue = 0.001f;
			e.FindPropertyRelative("sensitivity").floatValue = 1000f;
			e.FindPropertyRelative("snap").boolValue = true;
			e.FindPropertyRelative("invert").boolValue = false;
			e.FindPropertyRelative("type").intValue = 0;
			e.FindPropertyRelative("axis").intValue = 0;
			e.FindPropertyRelative("joyNum").intValue = 0;
		}
		Eje("Horizontal", "left", "right", "a", "d");
		Eje("Vertical", "down", "up", "s", "w");
		Eje("Jump", "", "space", "", "z");
		Eje("Submit", "", "return", "", "enter");
		Eje("Cancel", "", "escape");
		inputManager.ApplyModifiedProperties();
	}

	static void CrearJuegoPrefab() {
		var go = new GameObject("Juego");
		var j = go.AddComponent<Juego>();
		j.fuenteMusica = go.AddComponent<AudioSource>();
		j.fuenteMusica.playOnAwake = false;
		j.fuenteMusica.loop = true;
		j.fuenteEfectos = go.AddComponent<AudioSource>();
		j.fuenteEfectos.playOnAwake = false;
		CrearEfectos(j);
		CrearMusica(j);
		juegoPrefab = PrefabUtility.SaveAsPrefabAsset(go, "Assets/Resources/Juego.prefab").GetComponent<Juego>();
		Object.DestroyImmediate(go);
	}

	// ------------------------------------------------ Utilidades de objetos

	static GameObject Objeto(string nombre, Transform padre = null, int capa = 0) {
		var go = new GameObject(nombre);
		go.layer = capa;
		if (padre != null) go.transform.SetParent(padre, false);
		return go;
	}

	static SpriteRenderer Render(GameObject go, Sprite s, int orden, Color? color = null) {
		var sr = go.AddComponent<SpriteRenderer>();
		sr.sprite = s;
		sr.sortingOrder = orden;
		if (color.HasValue) sr.color = color.Value;
		return sr;
	}

	static SpriteRenderer Grafico(GameObject padre, Sprite s, int orden) {
		var g = Objeto("Grafico", padre.transform, padre.layer);
		return Render(g, s, orden);
	}

	static SpriteRenderer Mosaico(GameObject go, Sprite s, Vector2 tamano, int orden) {
		var sr = Render(go, s, orden);
		sr.drawMode = SpriteDrawMode.Tiled;
		sr.tileMode = SpriteTileMode.Continuous;
		sr.size = tamano;
		return sr;
	}

	static Rigidbody2D Cuerpo(GameObject go, RigidbodyType2D tipo, float gravedad = 1f) {
		var rb = go.AddComponent<Rigidbody2D>();
		rb.bodyType = tipo;
		rb.gravityScale = gravedad;
		rb.constraints = RigidbodyConstraints2D.FreezeRotation;
		rb.interpolation = RigidbodyInterpolation2D.Interpolate;
		rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
		rb.sharedMaterial = sinFriccion;
		return rb;
	}

	static T Caja<T>(GameObject go, Vector2 tamano, Vector2 desplazamiento, bool trigger) where T : Collider2D {
		var c = go.AddComponent<T>();
		c.isTrigger = trigger;
		c.offset = desplazamiento;
		if (c is BoxCollider2D b) b.size = tamano;
		if (c is CircleCollider2D ci) ci.radius = tamano.x;
		c.sharedMaterial = sinFriccion;
		return c;
	}

	static ZonaGolpe Zona(GameObject padre, Enemigo e, Vector2 tamano, Vector2 desplazamiento) {
		var z = Objeto("ZonaGolpe", padre.transform, CAPA_GOLPE);
		Caja<BoxCollider2D>(z, tamano, desplazamiento, true);
		var zg = z.AddComponent<ZonaGolpe>();
		zg.enemigo = e;
		return zg;
	}

	static GameObject Guardar(GameObject go, string nombre) {
		var prefab = PrefabUtility.SaveAsPrefabAsset(go, "Assets/Prefabs/" + nombre + ".prefab");
		Object.DestroyImmediate(go);
		return prefab;
	}

	static GameObject Poner(GameObject prefab, Vector3 pos, Transform padre) {
		var go = (GameObject)PrefabUtility.InstantiatePrefab(prefab, padre);
		go.transform.position = pos;
		return go;
	}

	//Guarda la escena activa registrando los cambios hechos a las instancias de prefabs
	static void GuardarEscena(string nombre) {
		var escena = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
		foreach (var raiz in escena.GetRootGameObjects()) {
			foreach (var t in raiz.GetComponentsInChildren<Transform>(true)) {
				foreach (var comp in t.GetComponents<Component>()) {
					if (comp != null && PrefabUtility.IsPartOfPrefabInstance(comp)) PrefabUtility.RecordPrefabInstancePropertyModifications(comp);
				}
			}
		}
		EditorSceneManager.SaveScene(escena, "Assets/Scenes/" + nombre + ".unity");
	}

	static Tile CrearTile(string nombre, Sprite s) {
		string ruta = "Assets/Tiles/" + nombre + ".asset";
		var t = AssetDatabase.LoadAssetAtPath<Tile>(ruta);
		if (t == null) {
			t = ScriptableObject.CreateInstance<Tile>();
			AssetDatabase.CreateAsset(t, ruta);
		}
		t.sprite = s;
		t.colliderType = Tile.ColliderType.Grid;
		EditorUtility.SetDirty(t);
		return t;
	}
}
