using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;

//Prueba automática del juego en modo Play: camina, salta, golpea bloques, aplasta enemigos,
//recibe daño, cae al vacío, completa el nivel, muere (vuelve al nivel 1) y derrota al jefe.
//Uso: Unity -batchmode -projectPath . -executeMethod PruebaAutomatica.Ejecutar
public static class PruebaAutomatica {

	static IEnumerator<float> pasos;
	static double siguiente;
	static readonly List<string> resultados = new List<string>();
	static readonly List<string> errores = new List<string>();
	static int fallos;

	[MenuItem("Ciguin/Prueba automática")]
	public static void Ejecutar() {
		EditorSettings.enterPlayModeOptionsEnabled = true;
		EditorSettings.enterPlayModeOptions = EnterPlayModeOptions.DisableDomainReload;
		EditorSceneManager.OpenScene("Assets/Scenes/Nivel1.unity");
		Application.logMessageReceived += (msg, pila, tipo) => {
			if ((tipo == LogType.Exception || tipo == LogType.Error) && !pila.Contains("UnityEditor.Search")) errores.Add(msg + "\n" + pila);
		};
		pasos = Pasos();
		siguiente = EditorApplication.timeSinceStartup + 0.3;
		EditorApplication.update += Avanzar;
		EditorApplication.EnterPlaymode();
	}

	static void Avanzar() {
		if (!EditorApplication.isPlaying || EditorApplication.timeSinceStartup < siguiente) return;
		bool sigue;
		try {
			sigue = pasos.MoveNext();
		} catch (System.Exception e) {
			Revisar(false, "Excepción en la prueba: " + e);
			sigue = false;
		}
		if (sigue) {
			siguiente = EditorApplication.timeSinceStartup + pasos.Current;
			return;
		}
		EditorApplication.update -= Avanzar;
		foreach (var e in errores.Distinct()) Revisar(false, "Error en consola: " + e);
		resultados.Add(fallos == 0 ? "RESULTADO: TODO OK" : "RESULTADO: " + fallos + " FALLOS");
		File.WriteAllLines("Logs/prueba.txt", resultados);
		Debug.Log("PRUEBA\n" + string.Join("\n", resultados));
		EditorSettings.enterPlayModeOptionsEnabled = false;
		EditorApplication.ExitPlaymode();
		if (Application.isBatchMode) EditorApplication.Exit(fallos == 0 ? 0 : 1);
	}

	static void Revisar(bool ok, string que) {
		resultados.Add((ok ? "[OK]    " : "[FALLA] ") + que);
		if (!ok) fallos++;
	}

	static Rigidbody2D Rb => Jugador.I.GetComponent<Rigidbody2D>();

	static void Mover(float x, float y) {
		Rb.position = new Vector2(x, y);
		Jugador.I.transform.position = new Vector3(x, y, 0f);
		Rb.linearVelocity = Vector2.zero;
		typeof(Jugador).GetField("velX", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(Jugador.I, 0f);
	}

	static T Cercano<T>(float x) where T : Component =>
		Object.FindObjectsByType<T>(FindObjectsSortMode.None).OrderBy(c => Mathf.Abs(c.transform.position.x - x)).FirstOrDefault();

	static string Diag() => "pos=" + Jugador.I.transform.position + " vel=" + Rb.linearVelocity + " suelo=" + Jugador.I.EnSuelo + " vidas=" + Juego.I.vidas + " muerto=" + Jugador.I.Muerto;

	static void CongelarCangrejos() {
		foreach (var c in Object.FindObjectsByType<Cangrejo>(FindObjectsSortMode.None)) c.velocidad = 0f;
	}

	static string Escena => SceneManager.GetActiveScene().name;

	static IEnumerator<float> Pasos() {

		// ---------------- Nivel 1
		CongelarCangrejos();
		var suelo = GameObject.Find("ColisionesSuelo");
		var comp = suelo.GetComponent<CompositeCollider2D>();
		resultados.Add("DIAG capas ignoradas suelo-jugador=" + Physics2D.GetIgnoreLayerCollision(8, 9) + " formas=" + comp.shapeCount + " puntos=" + comp.pointCount + " solapa=" + (Physics2D.OverlapPoint(new Vector2(3.5f, 1.5f), 1 << 8) != null) + " capaJugador=" + Jugador.I.gameObject.layer + " rbTipo=" + suelo.GetComponent<Rigidbody2D>().bodyType + " sim=" + suelo.GetComponent<Rigidbody2D>().simulated + " audio=" + AudioSettings.driverCapabilities + " y=" + Jugador.I.transform.position.y);
		Revisar(Escena == "Nivel1", "Arranca en Nivel1");
		Revisar(Jugador.I != null && Nivel.I != null, "Existen Jugador y Nivel");
		Revisar(Jugador.I.EnSuelo, "Ciguín está parado sobre el suelo (y=" + Jugador.I.transform.position.y.ToString("0.00") + ")");
		Revisar(Juego.I.fuenteMusica.isPlaying && Juego.I.fuenteMusica.clip.name == "Musica_Nivel1", "Suena la música del nivel 1");

		float x0 = Jugador.I.transform.position.x;
		Entrada.Simular(1f, false);
		yield return 1f;
		Entrada.Simular(0f, false);
		Revisar(Jugador.I.transform.position.x - x0 > 4f, "Camina a la derecha (" + (Jugador.I.transform.position.x - x0).ToString("0.0") + " unidades en 1 s)");

		Mover(5.5f, 2.6f);
		yield return 0.4f;
		float yMax = 0f;
		Entrada.Simular(0f, true);
		for (int i = 0; i < 12; i++) {
			yMax = Mathf.Max(yMax, Jugador.I.transform.position.y);
			yield return 0.05f;
		}
		Entrada.Simular(0f, false);
		Revisar(yMax - 2.5f > 3.2f, "Salto alto (" + (yMax - 2.5f).ToString("0.0") + " unidades)");
		yield return 1f;

		var bloque = Cercano<Bloque>(9.5f);
		Mover(9.5f, 2.6f);
		yield return 0.3f;
		int lar0 = Juego.I.larimares;
		Entrada.Simular(0f, true);
		yield return 0.7f;
		Entrada.Simular(0f, false);
		yield return 0.6f;
		Revisar(bloque.tipo == Bloque.Tipo.Vacio && Juego.I.larimares == lar0 + 1, "Golpear bloque sorpresa da un larimar");

		var ladrillo = Cercano<Bloque>(10.5f);
		Revisar(ladrillo.tipo == Bloque.Tipo.Ladrillo, "Hay un ladrillo en x=10");
		Mover(10.5f, 2.6f);
		yield return 0.3f;
		Entrada.Simular(0f, true);
		yield return 0.7f;
		Entrada.Simular(0f, false);
		yield return 0.6f;
		Revisar(ladrillo == null, "Golpear ladrillo lo rompe");

		var cangrejo = Cercano<Cangrejo>(20f);
		int puntos0 = Juego.I.puntos;
		Mover(cangrejo.transform.position.x, cangrejo.transform.position.y + 2.5f);
		yield return 1.2f;
		Revisar(cangrejo == null && Juego.I.vidas == 3 && Juego.I.puntos > puntos0, "Aplastar cangrejo saltándole encima (sin perder vida)");

		var cangrejo2 = Cercano<Cangrejo>(28f);
		Mover(cangrejo2.transform.position.x - 0.55f, cangrejo2.transform.position.y + 0.1f);
		yield return 0.4f;
		Revisar(Juego.I.vidas == 2 && Jugador.I.Invulnerable, "Tocar cangrejo de lado quita una vida (vidas=" + Juego.I.vidas + ")");
		yield return 2.2f;

		Mover(36f, 5f);
		yield return 2.2f;
		Revisar(Juego.I.vidas == 1 && Jugador.I.transform.position.y > 1f, "Caer al agua quita una vida y reaparece (vidas=" + Juego.I.vidas + ")");
		yield return 2.2f;

		//Mango del bloque en (11,9): subir al bloque de x=11 y golpearlo
		Mover(11.5f, 6.6f);
		yield return 0.4f;
		Entrada.Simular(0f, true);
		yield return 0.8f;
		Entrada.Simular(0f, false);
		yield return 1.2f;
		var mango = Object.FindAnyObjectByType<Mango>();
		Revisar(mango != null, "El bloque M suelta un mango");
		if (mango != null) {
			Mover(mango.transform.position.x, mango.transform.position.y + 0.3f);
			yield return 0.5f;
		}
		Revisar(Juego.I.vidas == 2, "Comer el mango da una vida (vidas=" + Juego.I.vidas + ")");

		//Cactus y espinas existen
		Revisar(Object.FindObjectsByType<CactusLanzador>(FindObjectsSortMode.None).Length >= 3, "Hay cactus lanzadores");
		Revisar(Object.FindObjectsByType<Murcielago>(FindObjectsSortMode.None).Length >= 3, "Hay murciélagos");
		Revisar(Object.FindObjectsByType<ObjetoQueCae>(FindObjectsSortMode.None).Length >= 3, "Hay cocos que caen");

		//Ir a la meta
		Mover(205.5f, 2.6f);
		Entrada.Simular(1f, false);
		yield return 2f;
		Entrada.Simular(0f, false);
		Revisar(Nivel.I.Terminado, "Tocar la bandera completa el nivel");
		for (int i = 0; i < 120 && Escena != "Nivel2"; i++) yield return 0.1f;
		yield return 0.05f;
		CongelarCangrejos();
		Revisar(Escena == "Nivel2", "Pasa al Nivel 2 (escena=" + Escena + ")");

		// ---------------- Nivel 2
		yield return 1.5f;
		CongelarCangrejos();
		Revisar(Jugador.I.EnSuelo, "Nivel 2: Ciguín sobre el suelo " + Diag());
		Revisar(Juego.I.fuenteMusica.clip.name == "Musica_Nivel2", "Suena la música del nivel 2");
		typeof(Jugador).GetField("invulnerable", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(Jugador.I, 0f);
		Juego.I.vidas = 1;
		Mover(40.5f, 3.2f);
		yield return 1f;
		resultados.Add("DIAG puas " + Diag());
		Revisar(Nivel.I.Terminado && Nivel.I.txtMensaje.text.Contains("MUERTO"), "Sin vidas en el nivel 2: aparece '" + Nivel.I.txtMensaje.text + "'");
		yield return 5f;
		Revisar(Escena == "Nivel1" && Juego.I.vidas == 3, "Al morir en el nivel 2 vuelve al Nivel 1 con 3 vidas (escena=" + Escena + ")");

		// ---------------- Tiempo agotado en nivel 2 también regresa al 1
		SceneManager.LoadScene("Nivel2");
		yield return 1.5f;
		typeof(Nivel).GetProperty("Tiempo").SetValue(Nivel.I, 0.5f);
		yield return 1.5f;
		Revisar(Nivel.I.Terminado && Nivel.I.txtSubMensaje.text.Contains("tiempo"), "Se acaba el tiempo: muere");
		yield return 5f;
		Revisar(Escena == "Nivel1", "Sin tiempo en el nivel 2 vuelve al Nivel 1");

		// ---------------- Nivel 3: jefe
		SceneManager.LoadScene("Nivel3");
		yield return 1.5f;
		Revisar(Jugador.I.EnSuelo, "Nivel 3: Ciguín sobre el suelo");
		var jefe = Object.FindAnyObjectByType<Jefe>();
		Revisar(jefe != null, "Existe el Galipote");
		Juego.I.vidas = 5;
		Mover(121f, 2.6f);
		Entrada.Simular(1f, false);
		yield return 1f;
		Entrada.Simular(0f, false);
		yield return 1.5f;
		Revisar(GameObject.Find("MuroJefe") != null, "Se cierra el muro de la arena");
		Revisar(Juego.I.fuenteMusica.clip.name == "Musica_Jefe", "Cambia a la música del jefe (" + Juego.I.fuenteMusica.clip.name + ")");
		Revisar(Nivel.I.panelJefe.activeSelf, "Aparece la barra de vida del jefe");
		yield return 3f;

		var campoVida = typeof(Jefe).GetField("vida", BindingFlags.NonPublic | BindingFlags.Instance);
		int golpes = 0;
		for (int i = 0; i < 14 && jefe != null && (int)campoVida.GetValue(jefe) > 0; i++) {
			Juego.I.vidas = 5;
			int antes = (int)campoVida.GetValue(jefe);
			var b = jefe.GetComponent<Collider2D>().bounds;
			Mover(b.center.x, b.max.y + 0.8f);
			Rb.linearVelocity = new Vector2(0f, -6f);
			yield return 0.25f;
			if ((int)campoVida.GetValue(jefe) < antes) golpes++;
			Mover(Mathf.Clamp(jefe.transform.position.x + (jefe.transform.position.x > 135f ? -6f : 6f), 123f, 148f), 2.6f);
			yield return 1.2f;
		}
		Revisar(jefe != null && (int)campoVida.GetValue(jefe) <= 0, "El Galipote recibe " + golpes + " golpes y es derrotado");
		Juego.I.vidas = 5;
		yield return 14f;
		Revisar(Escena == "Victoria", "Tras derrotar al jefe aparece la pantalla de Victoria (escena=" + Escena + ")");
		yield return 1f;

		SceneManager.LoadScene("Menu");
		yield return 1.5f;
		Revisar(Escena == "Menu" && Juego.I.fuenteMusica.clip.name == "Musica_Menu", "El menú carga con su música");
		Entrada.DejarDeSimular();
	}
}
