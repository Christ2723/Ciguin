using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Tilemaps;

//Los tres niveles. Cada nivel se dibuja en una cuadrícula de caracteres (y=0 es abajo):
// #  suelo            S  piedra dura       B  ladrillo        ?  bloque con larimar
// V  bloque con varios larimares            M  bloque con mango
// o  larimar          C  cangrejo          m  murciélago      T  cactus lanzador
// k  coco/estalactita ^  púas              =  plataforma de madera
// ~  plataforma que cae                    H  plataforma móvil horizontal   I  vertical
// P  inicio de Ciguín X  punto de control  F  meta             J  jefe
public static partial class ConstructorProyecto {

	class Mapa {
		public readonly int ancho;
		public const int ALTO = 15;
		readonly char[,] c;
		public readonly List<(Sprite s, float x, float y, int orden, Color color)> deco = new List<(Sprite, float, float, int, Color)>();

		public Mapa(int ancho) {
			this.ancho = ancho;
			c = new char[ancho, ALTO];
			for (int x = 0; x < ancho; x++) for (int y = 0; y < ALTO; y++) c[x, y] = '.';
		}

		public char this[int x, int y] {
			get => x < 0 || y < 0 || x >= ancho || y >= ALTO ? '.' : c[x, y];
			set { if (x >= 0 && y >= 0 && x < ancho && y < ALTO) c[x, y] = value; }
		}

		public void Suelo(int x0, int x1, int alto = 2) => Rect(x0, 0, x1 - x0 + 1, alto, '#');

		public void Rect(int x, int y, int w, int h, char ch) {
			for (int i = 0; i < w; i++) for (int k = 0; k < h; k++) this[x + i, y + k] = ch;
		}

		public void Pon(int x, int y, string s) {
			for (int i = 0; i < s.Length; i++) if (s[i] != ' ') this[x + i, y] = s[i];
		}

		public void EscaleraSube(int x, int y, int pasos) {
			for (int i = 0; i < pasos; i++) Rect(x + i, y, 1, i + 1, 'S');
		}

		public void EscaleraBaja(int x, int y, int pasos) {
			for (int i = 0; i < pasos; i++) Rect(x + i, y, 1, pasos - i, 'S');
		}

		public void Deco(Sprite s, float x, float y, int orden = -5, Color? color = null) => deco.Add((s, x, y, orden, color ?? Color.white));

		public bool Solido(int x, int y) => this[x, y] == '#' || this[x, y] == 'S';
	}

	// ------------------------------------------------ Nivel 1: Playa de Samaná

	static void CrearNivel1() {
		var m = new Mapa(222);
		m.Suelo(0, 34);
		m.Pon(3, 2, "P");
		m.Pon(9, 5, "?B?B?");
		m.Pon(11, 9, "M");
		m.Pon(16, 3, "ooo");
		m.Pon(20, 2, "C");
		m.Rect(24, 2, 1, 2, 'S');
		m.Pon(28, 2, "C");
		m.Pon(31, 6, "k");
		//hoyo 35-37
		m.Pon(35, 6, "ooo");
		m.Suelo(38, 70);
		m.Pon(44, 2, "^^");
		m.Pon(43, 6, "oooo");
		m.Pon(50, 2, "C");
		m.Pon(54, 2, "C");
		m.Pon(57, 5, "===");
		m.Pon(57, 7, "ooo");
		m.Pon(64, 2, "T");
		m.Pon(68, 6, "k");
		//hoyo 71-76 con plataforma móvil
		m.Pon(71, 3, "H");
		m.Suelo(77, 110);
		m.EscaleraSube(80, 2, 3);
		m.Rect(83, 2, 2, 3, 'S');
		m.EscaleraBaja(85, 2, 3);
		m.Pon(83, 7, "oo");
		m.Pon(92, 8, "m");
		m.Pon(96, 2, "X");
		m.Pon(99, 5, "B?B");
		m.Pon(100, 9, "V");
		m.Pon(104, 2, "C");
		m.Pon(108, 6, "k");
		//hoyo 111-114 con plataforma que cae
		m.Pon(112, 4, "~~");
		m.Suelo(115, 150);
		m.Pon(122, 2, "T");
		m.Pon(126, 5, "?BBB?");
		m.Pon(127, 9, "ooo");
		m.Pon(130, 9, "m");
		m.Pon(134, 2, "C");
		m.Pon(137, 2, "C");
		m.Pon(142, 2, "^^^");
		m.Pon(141, 5, "=====");
		m.Pon(142, 6, "ooo");
		//hoyo 151-156
		m.Pon(151, 4, "==");
		m.Pon(154, 6, "==");
		m.Pon(154, 8, "oo");
		m.Suelo(157, 221);
		m.Pon(165, 8, "m");
		m.Pon(168, 2, "C");
		m.Pon(171, 2, "C");
		m.Pon(175, 5, "?M?");
		m.Pon(178, 2, "T");
		m.Pon(182, 6, "ooooo");
		m.Pon(187, 2, "C");
		m.EscaleraSube(194, 2, 8);
		m.Rect(202, 2, 1, 8, 'S');
		m.Pon(209, 2, "F");

		var tema = temas[0];
		foreach (var x in new[] { 6, 48, 90, 130, 160, 186, 205 }) m.Deco(sPalmera, x + 0.5f, 2f, -6);
		foreach (var x in new[] { 31, 68, 108 }) m.Deco(sPalmera, x + 0.5f, 2f, -6);
		foreach (var x in new[] { 13, 40, 60, 88, 118, 145, 170, 190 }) m.Deco(sArbusto, x, 2f, -4);
		m.Deco(sCasita1, 22f, 2f, -7);
		m.Deco(sCasita2, 117f, 2f, -7);
		m.Deco(sCasita1, 162f, 2f, -7);
		m.Deco(sCasita2, 215f, 2f, -7);

		ConstruirNivel(1, "Playa de Samaná", m, tema, 200f, mNivel1, null, "Nivel2", pCoco, true);
	}

	// ------------------------------------------------ Nivel 2: Cueva de las Maravillas

	static void CrearNivel2() {
		var m = new Mapa(212);
		m.Rect(0, 13, 212, 2, '#'); //techo
		m.Suelo(0, 25);
		m.Pon(3, 2, "P");
		m.Pon(8, 5, "B?B?B");
		m.Pon(10, 9, "M");
		m.Pon(21, 2, "C");
		m.Pon(18, 12, "k");
		m.Pon(22, 12, "k");
		//hoyo 26-29
		m.Pon(26, 6, "oooo");
		m.Suelo(30, 55);
		m.Pon(36, 7, "m");
		m.Pon(40, 2, "^^");
		m.Pon(39, 6, "oooo");
		m.Pon(45, 2, "C");
		m.Pon(48, 2, "C");
		m.Pon(52, 2, "T");
		//hoyo 56-63 con plataforma móvil
		m.Pon(56, 4, "H");
		m.Suelo(64, 100);
		m.Rect(72, 2, 7, 2, '#');
		m.Pon(68, 12, "k");
		m.Pon(75, 9, "m");
		m.Pon(73, 4, "ooooo");
		m.Pon(82, 2, "C");
		m.Pon(86, 5, "BBBBB");
		m.Pon(88, 9, "V");
		m.Pon(92, 2, "X");
		m.Pon(96, 2, "^^^");
		m.Pon(95, 6, "=====");
		//hoyo 101-108 con plataformas que caen
		m.Pon(102, 4, "~~");
		m.Pon(105, 5, "~~");
		m.Suelo(109, 150);
		m.Pon(115, 7, "m");
		m.Pon(122, 8, "m");
		m.Rect(125, 8, 11, 5, '#'); //pasillo bajo
		m.Pon(128, 2, "C");
		m.Pon(132, 2, "C");
		m.Pon(126, 3, "oooooooo");
		m.Pon(140, 2, "T");
		m.Pon(144, 5, "?B?");
		//hoyo 151-158 con plataforma móvil
		m.Pon(151, 4, "H");
		m.Suelo(159, 211);
		m.Pon(165, 12, "k");
		m.Pon(168, 12, "k");
		m.Pon(171, 12, "k");
		m.Pon(175, 2, "C");
		m.Pon(180, 8, "m");
		m.Pon(183, 2, "C");
		m.Pon(186, 2, "T");
		m.EscaleraSube(190, 2, 6);
		m.Rect(196, 2, 1, 6, 'S');
		m.Pon(203, 2, "F");

		foreach (var x in new[] { 5, 20, 34, 47, 66, 81, 112, 138, 162, 178, 199 }) m.Deco(sCristal, x + 0.5f, 2f, -4);
		foreach (var x in new[] { 12, 43, 70, 90, 118, 147, 170, 185, 207 }) m.Deco(sHongo, x + 0.5f, 2f, -4);
		foreach (var x in new[] { 9, 28, 58, 85, 104, 120, 155, 192 }) m.Deco(sEstalagmita, x + 0.5f, 2f, -6, new Color(0.7f, 0.7f, 0.85f));

		ConstruirNivel(2, "Cueva de las Maravillas", m, temas[1], 200f, mNivel2, null, "Nivel3", pEstalactita, false);
	}

	// ------------------------------------------------ Nivel 3: Loma del Galipote (jefe)

	static void CrearNivel3() {
		var m = new Mapa(150);
		m.Suelo(0, 30);
		m.Pon(3, 2, "P");
		m.Pon(8, 5, "?M?");
		m.Pon(14, 2, "C");
		m.Pon(20, 2, "T");
		m.Pon(25, 6, "k");
		//hoyo 31-34
		m.Pon(31, 6, "oooo");
		m.Suelo(35, 60);
		m.Pon(40, 8, "m");
		m.Pon(45, 2, "^^");
		m.Pon(47, 5, "B?B");
		m.Pon(50, 2, "C");
		m.Pon(54, 2, "C");
		m.Pon(57, 6, "k");
		//hoyo 61-66 con plataformas que caen
		m.Pon(62, 4, "~~");
		m.Pon(65, 5, "~~");
		m.Suelo(67, 100);
		m.Pon(72, 2, "T");
		m.Pon(78, 8, "m");
		m.Pon(84, 2, "X");
		m.Pon(88, 2, "C");
		m.Pon(90, 5, "?V?");
		m.Pon(92, 2, "C");
		m.Pon(96, 9, "m");
		//hoyo 101-104
		m.Pon(102, 5, "==");
		m.Suelo(105, 149);
		m.Pon(110, 5, "?M?");
		m.Pon(114, 2, "T");
		m.Pon(117, 6, "ooo");
		//Arena del jefe (122 a 148), cerrada por la derecha
		m.Rect(149, 2, 1, 13, 'S');
		m.Pon(127, 6, "===");
		m.Pon(141, 6, "===");
		m.Pon(140, 2, "J");

		var noche = new Color(0.45f, 0.5f, 0.75f);
		foreach (var x in new[] { 25, 57 }) m.Deco(sPalmera, x + 0.5f, 2f, -6, noche);
		foreach (var x in new[] { 6, 38, 70, 98, 112 }) m.Deco(sArbolSeco, x + 0.5f, 2f, -6);
		foreach (var x in new[] { 17, 52, 80, 118 }) m.Deco(sCruz, x + 0.5f, 2f, -5);
		foreach (var x in new[] { 11, 44, 75, 108, 126, 145 }) m.Deco(sArbusto, x, 2f, -4, noche);

		ConstruirNivel(3, "Loma del Galipote", m, temas[2], 240f, mNivel3, mJefe, "Victoria", pCoco, false);
	}

	// ------------------------------------------------ Construcción de una escena de nivel

	static void ConstruirNivel(int numero, string nombre, Mapa m, Tema tema, float tiempo, AudioClip musica, AudioClip musicaJefe,
		string siguiente, GameObject prefabQueCae, bool nubes) {

		EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

		var raizNivel = Objeto("Nivel").transform;
		var raizEnemigos = Objeto("Enemigos").transform;
		var raizObjetos = Objeto("Objetos").transform;
		var raizDeco = Objeto("Decoracion").transform;

		// ---- Suelo con Tilemap
		var grid = Objeto("Grid");
		grid.AddComponent<Grid>();
		var tmGo = Objeto("Suelo", grid.transform, CAPA_SUELO);
		var tm = tmGo.AddComponent<Tilemap>();
		tmGo.AddComponent<TilemapRenderer>().sortingOrder = 0;

		var tSup = CrearTile(tema.nombre + "_Superficie", tema.superficie);
		var tInt = CrearTile(tema.nombre + "_Interior", tema.interior);
		var tPie = CrearTile(tema.nombre + "_Piedra", tema.piedra);

		Vector3 inicio = new Vector3(3.5f, 2.5f, 0f);
		Jefe jefe = null;

		for (int x = 0; x < m.ancho; x++) {
			for (int y = 0; y < Mapa.ALTO; y++) {
				char ch = m[x, y];
				Vector3 centro = new Vector3(x + 0.5f, y + 0.5f, 0f);
				switch (ch) {
					case '#': tm.SetTile(new Vector3Int(x, y, 0), m[x, y + 1] == '#' || y == Mapa.ALTO - 1 ? tInt : tSup); break;
					case 'S': tm.SetTile(new Vector3Int(x, y, 0), tPie); break;
					case 'B':
					case '?':
					case 'V':
					case 'M': {
						var go = Poner(pBloque, centro, raizNivel);
						var b = go.GetComponent<Bloque>();
						if (ch == 'B') {
							b.tipo = Bloque.Tipo.Ladrillo;
							b.render.sprite = tema.ladrillo;
						} else {
							b.tipo = Bloque.Tipo.Sorpresa;
							b.contenido = ch == 'M' ? Bloque.Contenido.Mango : ch == 'V' ? Bloque.Contenido.VariosLarimares : Bloque.Contenido.Larimar;
						}
						break;
					}
					case 'o': Poner(pLarimar, centro, raizObjetos); break;
					case 'C': Poner(pCangrejo, centro + Vector3.down * 0.1f, raizEnemigos); break;
					case 'm': Poner(pMurcielago, centro, raizEnemigos); break;
					case 'T': Poner(pCactus, centro, raizEnemigos); break;
					case 'k': Poner(prefabQueCae, centro, raizEnemigos); break;
					case '^': Poner(pPuas, centro, raizEnemigos); break;
					case '=':
					case '~': {
						if (m[x - 1, y] == ch) break;
						int largo = 1;
						while (m[x + largo, y] == ch) largo++;
						var go = Poner(ch == '=' ? pPlataforma : pPlataformaCae, new Vector3(x + largo / 2f, y + 0.5f, 0f), raizNivel);
						if (ch == '=') {
							go.GetComponent<SpriteRenderer>().size = new Vector2(largo, 0.5f);
							go.GetComponent<BoxCollider2D>().size = new Vector2(largo, 0.3f);
						} else {
							go.GetComponentInChildren<SpriteRenderer>().size = new Vector2(largo, 0.5f);
							go.GetComponent<BoxCollider2D>().size = new Vector2(largo, 0.36f);
						}
						break;
					}
					case 'H':
					case 'I': {
						var go = Poner(pPlataformaMovil, new Vector3(x + 1.5f, y + 0.5f, 0f), raizNivel);
						var pm = go.GetComponent<PlataformaMovil>();
						pm.desplazamiento = ch == 'H' ? new Vector2(3.5f, 0f) : new Vector2(0f, 4f);
						pm.periodo = 4.5f;
						break;
					}
					case 'P': inicio = new Vector3(x + 0.5f, y + 0.5f, 0f); break;
					case 'X': Poner(pPuntoControl, new Vector3(x + 0.3f, y, 0f), raizObjetos); break;
					case 'F': Poner(pMeta, new Vector3(x + 0.5f, y, 0f), raizObjetos); break;
					case 'J': jefe = Poner(pJefe, new Vector3(x + 1f, y + 1.45f, 0f), raizEnemigos).GetComponent<Jefe>(); break;
				}
			}
		}

		// ---- Colisiones del suelo: rectángulos fusionados (filas horizontales que se extienden hacia arriba)
		var colisiones = Objeto("ColisionesSuelo", grid.transform, CAPA_SUELO);
		var rbSuelo = colisiones.AddComponent<Rigidbody2D>();
		rbSuelo.bodyType = RigidbodyType2D.Static;
		var compuesto = colisiones.AddComponent<CompositeCollider2D>();
		compuesto.geometryType = CompositeCollider2D.GeometryType.Polygons;
		compuesto.sharedMaterial = sinFriccion;
		var usado = new bool[m.ancho, Mapa.ALTO];
		for (int y = 0; y < Mapa.ALTO; y++) {
			for (int x = 0; x < m.ancho; x++) {
				if (!m.Solido(x, y) || usado[x, y]) continue;
				int w = 0;
				while (x + w < m.ancho && m.Solido(x + w, y) && !usado[x + w, y]) w++;
				int h = 1;
				while (y + h < Mapa.ALTO) {
					bool fila = true;
					for (int i = 0; i < w && fila; i++) fila = m.Solido(x + i, y + h) && !usado[x + i, y + h];
					if (!fila || m.Solido(x - 1, y + h) && !usado[x - 1, y + h] || m.Solido(x + w, y + h) && !usado[x + w, y + h]) break;
					h++;
				}
				for (int i = 0; i < w; i++) for (int k = 0; k < h; k++) usado[x + i, y + k] = true;
				var caja = colisiones.AddComponent<BoxCollider2D>();
				caja.size = new Vector2(w, h);
				caja.offset = new Vector2(x + w / 2f, y + h / 2f);
				caja.compositeOperation = Collider2D.CompositeOperation.Merge;
			}
		}

		// ---- Agua en los hoyos
		for (int x = 0; x < m.ancho; x++) {
			if (m.Solido(x, 0) || (x > 0 && !m.Solido(x - 1, 0))) continue;
			int largo = 0;
			while (x + largo < m.ancho && !m.Solido(x + largo, 0)) largo++;
			var agua = Objeto("Agua", raizDeco);
			agua.transform.position = new Vector3(x + largo / 2f, 0.5f, 0f);
			Mosaico(agua, tema.agua[0], new Vector2(largo, 1f), 20);
			var an = agua.AddComponent<AnimadorSprite>();
			an.frames = tema.agua;
			an.fps = 2.5f;
		}

		// ---- Decoración
		foreach (var d in m.deco) {
			var go = Objeto(d.s.name, raizDeco);
			go.transform.position = new Vector3(d.x, d.y, 0f);
			Render(go, d.s, d.orden, d.color);
		}
		if (numero == 3) {
			var r = new System.Random(5);
			for (int i = 0; i < 26; i++) {
				var l = Objeto("Luciernaga", raizDeco);
				l.transform.position = new Vector3((float)r.NextDouble() * m.ancho, 2.8f + (float)r.NextDouble() * 6f, 0f);
				Render(l, sLuciernaga[0], 15);
				var a = l.AddComponent<AnimadorSprite>();
				a.frames = sLuciernaga;
				a.fps = 3f;
				var f = l.AddComponent<Flotar>();
				f.amplitud = 0.3f;
				f.frecuencia = 1.2f + (float)r.NextDouble();
			}
		}

		// ---- Ciguín y cámara
		var jugador = Poner(pJugador, inicio, null);
		var camara = CrearCamara(tema, m.ancho, jugador.transform, nubes, numero == 3);

		// ---- Arena del jefe
		if (jefe != null) {
			jefe.limiteIzquierdo = 122f;
			jefe.limiteDerecho = 149f;
			var muro = Objeto("MuroJefe", raizNivel, CAPA_SUELO);
			muro.transform.position = new Vector3(121.5f, 8.5f, 0f);
			Mosaico(muro, tema.piedra, new Vector2(1f, 13f), 1);
			Caja<BoxCollider2D>(muro, new Vector2(1f, 13f), Vector2.zero, false);
			muro.SetActive(false);
			var zona = Objeto("ZonaJefe", raizNivel, CAPA_RECOGIBLE);
			zona.transform.position = new Vector3(124.5f, 7.5f, 0f);
			Caja<BoxCollider2D>(zona, new Vector2(1f, 15f), Vector2.zero, true);
			var zj = zona.AddComponent<ZonaJefe>();
			zj.jefe = jefe;
			zj.muro = muro;
			zj.camaraMinX = 122f + 13.34f;
		}

		// ---- Controlador del nivel y HUD
		var nivelGo = Objeto("ControlNivel");
		var nivel = nivelGo.AddComponent<Nivel>();
		nivel.numero = numero;
		nivel.nombre = nombre;
		nivel.tiempoLimite = tiempo;
		nivel.escenaSiguiente = siguiente;
		nivel.musica = musica;
		nivel.musicaJefe = musicaJefe;
		nivel.anchoNivel = m.ancho;
		nivel.fuente = fuente;
		CrearHUD(nivel, numero == 1);

		GuardarEscena("Nivel" + numero);
	}

	//Cámara ortográfica con cielo, fondos con parallax, nubes, luna y estrellas
	static Camera CrearCamara(Tema tema, float ancho, Transform objetivo, bool nubes, bool noche) {

		var camGo = Objeto("Main Camera");
		camGo.tag = "MainCamera";
		var cam = camGo.AddComponent<Camera>();
		cam.orthographic = true;
		cam.orthographicSize = 7.5f;
		cam.clearFlags = CameraClearFlags.SolidColor;
		cam.backgroundColor = Color.black;
		cam.nearClipPlane = 0.3f;
		cam.farClipPlane = 50f;
		camGo.AddComponent<AudioListener>();
		camGo.transform.position = new Vector3(Mathf.Max(13.34f, objetivo != null ? objetivo.position.x : 13.34f), 7.5f, -10f);
		if (objetivo != null) {
			var seg = camGo.AddComponent<CamaraSeguir>();
			seg.objetivo = objetivo;
			seg.minX = 13.34f;
			seg.maxX = ancho - 13.34f;
		}

		var cielo = Objeto("Cielo", camGo.transform);
		cielo.transform.localPosition = new Vector3(0f, 0f, 20f);
		cielo.transform.localScale = new Vector3(130f, 4f, 1f);
		Render(cielo, tema.cielo, -100);

		if (noche) {
			var est = Objeto("Estrellas", camGo.transform);
			est.transform.localPosition = new Vector3(0f, 0f, 19f);
			Mosaico(est, sEstrellas, new Vector2(32f, 16f), -99);
			var luna = Objeto("Luna", camGo.transform);
			luna.transform.localPosition = new Vector3(8.5f, 4.2f, 18f);
			Render(luna, sLuna, -98);
		}

		float anchoFondo = ancho + 80f;
		float yFondo = tema.nombre == "Cueva" ? 0f : 1.5f;
		var lejano = Objeto("FondoLejano");
		lejano.transform.position = new Vector3(0f, yFondo, 5f);
		Mosaico(lejano, tema.lejano, new Vector2(anchoFondo, tema.lejano.rect.height / 16f), -60);
		lejano.AddComponent<Parallax>().factor = 0.85f;

		var medio = Objeto("FondoMedio");
		medio.transform.position = new Vector3(0f, yFondo, 4f);
		Mosaico(medio, tema.medio, new Vector2(anchoFondo, tema.medio.rect.height / 16f), -50);
		medio.AddComponent<Parallax>().factor = 0.6f;

		if (nubes) {
			var capa = Objeto("Nubes");
			var p = capa.AddComponent<Parallax>();
			p.factor = 0.92f;
			p.desplazamientoAuto = -0.25f;
			var r = new System.Random(11);
			for (int i = 0; i < 22; i++) {
				var n = Objeto("Nube", capa.transform);
				n.transform.localPosition = new Vector3(-20f + i * 9f + (float)r.NextDouble() * 4f, 9.5f + (float)r.NextDouble() * 4f, 3f);
				Render(n, sNube, -70);
			}
		}
		return cam;
	}
}
