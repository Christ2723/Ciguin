using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

//Arte del juego: todo se dibuja aquí en pixel art (16 px = 1 unidad) para que los personajes sean originales.
public static partial class ConstructorProyecto {

	// ------------------------------------------------ Utilidades de píxeles

	static Color Hex(string h) {
		ColorUtility.TryParseHtmlString("#" + h, out var c);
		return c;
	}

	static readonly Color CONTORNO = Hex("2a1a12");

	static readonly Dictionary<char, Color> paletaBase = new Dictionary<char, Color> {
		{ 'K', Hex("2a1a12") }, { 'H', Hex("f0cc62") }, { 'h', Hex("c9a03c") }, { 'R', Hex("d8342a") }, { 'r', Hex("9e2219") },
		{ 'B', Hex("2c56b8") }, { 'G', Hex("7aa83c") }, { 'g', Hex("4d7426") }, { 'W', Hex("f7f2e2") }, { 's', Hex("9a6a44") },
		{ 'E', Hex("111111") }, { 'e', Hex("ffffff") }, { 'Y', Hex("f5a623") }, { 'O', Hex("e8792a") }, { 'P', Hex("6b3fa0") },
		{ 'p', Hex("4a2878") }, { 'y', Hex("ffe14d") }, { 'w', Hex("ffffff") }, { 'C', Hex("4caf50") }, { 'c', Hex("a5e07a") },
		{ 'F', Hex("ff6fa8") }, { 'T', Hex("c0623a") }, { 'L', Hex("e05a40") }, { 'l', Hex("ff8a6a") },
	};

	static float Hash(int x, int y, int s) {
		unchecked {
			int h = x * 374761393 + y * 668265263 + s * 982451653;
			h = (h ^ (h >> 13)) * 1274126177;
			h ^= h >> 16;
			return (h & 0xFFFFFF) / (float)0x1000000;
		}
	}

	static Sprite GuardarSprite(string nombre, Texture2D tex, Vector2 pivote, bool repetir = false, Vector4 borde = default, int ppu = 16) {
		string ruta = "Assets/Sprites/" + nombre + ".png";
		File.WriteAllBytes(ruta, tex.EncodeToPNG());
		UnityEngine.Object.DestroyImmediate(tex);
		AssetDatabase.ImportAsset(ruta, ImportAssetOptions.ForceSynchronousImport);
		var imp = (TextureImporter)AssetImporter.GetAtPath(ruta);
		imp.textureType = TextureImporterType.Sprite;
		imp.spriteImportMode = SpriteImportMode.Single;
		imp.spritePixelsPerUnit = ppu;
		imp.filterMode = FilterMode.Point;
		imp.textureCompression = TextureImporterCompression.Uncompressed;
		imp.mipmapEnabled = false;
		imp.alphaIsTransparency = true;
		imp.wrapMode = repetir ? TextureWrapMode.Repeat : TextureWrapMode.Clamp;
		var s = new TextureImporterSettings();
		imp.ReadTextureSettings(s);
		s.spriteMeshType = SpriteMeshType.FullRect;
		s.spriteAlignment = (int)SpriteAlignment.Custom;
		s.spritePivot = pivote;
		s.spriteBorder = borde;
		s.spriteExtrude = 1;
		imp.SetTextureSettings(s);
		imp.SaveAndReimport();
		return AssetDatabase.LoadAssetAtPath<Sprite>(ruta);
	}

	//Sprite a partir de filas de texto (la fila 0 es la de arriba)
	static Sprite Pixel(string nombre, string[] filas, Dictionary<char, Color> extra = null, Vector2? pivote = null, int ancho = 0) {
		int h = filas.Length;
		int w = ancho > 0 ? ancho : 0;
		if (w == 0) foreach (var f in filas) w = Mathf.Max(w, f.Length);
		var tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
		for (int fila = 0; fila < h; fila++) {
			for (int x = 0; x < w; x++) {
				char ch = x < filas[fila].Length ? filas[fila][x] : '.';
				Color c = Color.clear;
				if (extra != null && extra.TryGetValue(ch, out var ce)) c = ce;
				else if (paletaBase.TryGetValue(ch, out var cb)) c = cb;
				tex.SetPixel(x, h - 1 - fila, c);
			}
		}
		return GuardarSprite(nombre, tex, pivote ?? new Vector2(0.5f, 0.5f));
	}

	//Sprite procedural: la función recibe x,y en píxeles (y=0 abajo) y devuelve un color o transparente.
	//Si contorno=true se agrega un borde oscuro alrededor de la figura.
	static Sprite Proc(string nombre, int w, int h, Func<int, int, Color> f, bool contorno = false, Vector2? pivote = null,
		bool repetir = false, Vector4 borde = default) {
		var px = new Color[w, h];
		for (int y = 0; y < h; y++) for (int x = 0; x < w; x++) px[x, y] = f(x, y);
		var tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
		for (int y = 0; y < h; y++) {
			for (int x = 0; x < w; x++) {
				Color c = px[x, y];
				if (contorno && c.a < 0.5f) {
					bool vecino = false;
					for (int dy = -1; dy <= 1 && !vecino; dy++) for (int dx = -1; dx <= 1 && !vecino; dx++) {
						if (Mathf.Abs(dx) + Mathf.Abs(dy) != 1) continue;
						int nx = x + dx, ny = y + dy;
						if (nx >= 0 && ny >= 0 && nx < w && ny < h && px[nx, ny].a >= 0.5f) vecino = true;
					}
					if (vecino) c = CONTORNO;
				}
				tex.SetPixel(x, y, c);
			}
		}
		return GuardarSprite(nombre, tex, pivote ?? new Vector2(0.5f, 0.5f), repetir, borde);
	}

	static bool Elipse(float x, float y, float cx, float cy, float rx, float ry) {
		float dx = (x - cx) / rx, dy = (y - cy) / ry;
		return dx * dx + dy * dy <= 1f;
	}

	static Color Mezcla(Color a, Color b, float t) => Color.Lerp(a, b, Mathf.Clamp01(t));

	// ------------------------------------------------ Sprites generados

	static Sprite[] sJugadorQuieto, sJugadorCaminar;
	static Sprite sJugadorSaltar, sJugadorCaer, sJugadorHerido, sJugadorCelebrar;
	static Sprite[] sCangrejo, sMurcielago, sCactus, sJefe;
	static Sprite sCactusDisparo, sJefeAtaque, sJefeHerido;
	static Sprite sCoco, sEstalactita, sEspina, sPiedra, sPuas;
	static Sprite[] sFuego;
	static Sprite[] sSorpresa;
	static Sprite sVacio, sEscombro;
	static Sprite sLarimar, sMango, sCorazon, sPlataforma, sPlataformaCae;
	static Sprite[] sLarimarGira, sBrillo, sPolvo, sExplosion, sAgua;
	static Sprite sPoste, sBanderaMeta, sPuntoControl, sPuntoControlActivo;
	static Sprite sPanel, sBoton, sBlanco;

	class Tema {
		public string nombre;
		public Sprite superficie, interior, piedra, ladrillo, cielo, lejano, medio, extra;
		public Sprite[] agua;
		public Color colorAgua;
	}
	static Tema[] temas;
	static Sprite sPalmera, sArbusto, sCasita1, sCasita2, sNube, sCristal, sHongo, sEstalagmita, sArbolSeco, sLuna, sEstrellas, sCruz;
	static Sprite[] sLuciernaga;

	static void CrearArte() {
		CrearJugador();
		CrearEnemigos();
		CrearJefeArte();
		CrearObjetos();
		CrearTemas();
		CrearDecoracion();
		CrearInterfazArte();
	}

	// ------------------------------------------------ Ciguín

	static void CrearJugador() {

		string[] cuerpo = {
			"................",
			"......KKKK......",
			".....KHHHHK.....",
			".....KRRRRK.....",
			"..KHHHHHHHHHHK..",
			"...KKKKKKKKKK...",
			"....KGGGGGeEK...",
			"....KGGGGGGGKYY.",
			"....KGGGGGGGYYYK",
			"....KRRRRRRRKKK.",
			"...KgGKWWsWWK...",
			"..KggGKWWWWsWK..",
			"..KgggKWsWWWWK..",
			"...KKKGWWWWWK...",
			"....KKKKKKKK....",
			".....KOK.KOK....",
		};

		string[] Variar(params (int fila, string texto)[] cambios) {
			var copia = (string[])cuerpo.Clone();
			foreach (var c in cambios) copia[c.fila] = c.texto;
			return copia;
		}

		var parpadeo = Variar((6, "....KGGGGGKKK..."));
		var paso1 = Variar((15, "....KOK...KOK..."));
		var paso2 = Variar((15, "......KOKKOK...."));
		var paso3 = Variar((14, "....KKKKKKKK...."), (15, ".....KOKKOK....."));
		var alas = new (int, string)[] {
			(10, ".KgggKWWsWWWK..."),
			(11, "..KKgKWWWWsWK..."),
			(12, "....KKWsWWWWK..."),
		};
		var salto = Variar(alas[0], alas[1], alas[2], (14, "....KKKKKKKK...."), (15, "......KOOK......"));
		var caer = Variar(alas[0], alas[1], alas[2]);
		var herido = Variar((6, "....KGGGGEGEK..."), (7, "....KGGGGGEGKYY."), (8, "....KGGGGGGGYKYK"));
		var celebrar = Variar(alas[0], alas[1], alas[2], (6, "....KGGGGKGKK..."), (15, "......KOOK......"));

		sJugadorQuieto = new[] { Pixel("Ciguin_Quieto", cuerpo), Pixel("Ciguin_Parpadeo", parpadeo) };
		sJugadorCaminar = new[] { Pixel("Ciguin_Paso1", paso1), Pixel("Ciguin_Paso2", paso3), Pixel("Ciguin_Paso3", paso2), Pixel("Ciguin_Paso4", paso3) };
		sJugadorSaltar = Pixel("Ciguin_Salto", salto);
		sJugadorCaer = Pixel("Ciguin_Caida", caer);
		sJugadorHerido = Pixel("Ciguin_Herido", herido);
		sJugadorCelebrar = Pixel("Ciguin_Celebra", celebrar);
	}

	// ------------------------------------------------ Enemigos

	static void CrearEnemigos() {

		//Cangrejo rojo (16x12)
		string[] cangrejoBase = {
			".KKK........KKK.",
			"KRRRK......KRRRK",
			"KRKRK......KRKRK",
			".KRK..KKKK..KRK.",
			"..KK.KeKKeK.KK..",
			"...KKRRRRRRKK...",
			"..KRRRRRRRRRRK..",
			".KRRlRRRRRRlRRK.",
			".KRRRRRRRRRRRRK.",
			"..KKKKKKKKKKKK..",
			".K.K.K....K.K.K.",
			"K.K.K......K.K.K",
		};
		var cangrejoB = (string[])cangrejoBase.Clone();
		cangrejoB[10] = "..K.K.K..K.K.K..";
		cangrejoB[11] = ".K.K.K....K.K.K.";
		cangrejoB[1] = "KRRRK......KRRRK";
		cangrejoB[2] = ".KRRK......KRRK.";
		var colCangrejo = new Dictionary<char, Color> { { 'R', Hex("e04a2e") }, { 'l', Hex("ff9a70") } };
		sCangrejo = new[] { Pixel("Cangrejo_1", cangrejoBase, colCangrejo), Pixel("Cangrejo_2", cangrejoB, colCangrejo) };

		//Murciélago (16x10)
		string[] murcA = {
			"KK............KK",
			"KPK..........KPK",
			"KPPK..K..K..KPPK",
			".KPPKKPKKPKKPPK.",
			".KPPPPPPPPPPPPK.",
			"..KPPPyKKyPPPK..",
			"...KKPPPPPPKK...",
			".....KPwwPK.....",
			"......KKKK......",
			"................",
		};
		string[] murcB = {
			"................",
			"................",
			"......K..K......",
			".....KPKKPK.....",
			"..KKKPPPPPPKKK..",
			".KPPPPyKKyPPPPK.",
			"KPPKKPPPPPPKKPPK",
			"KPK..KPwwPK..KPK",
			"KK....KKKK....KK",
			"K..............K",
		};
		sMurcielago = new[] { Pixel("Murcielago_1", murcA), Pixel("Murcielago_2", murcB) };

		//Cactus lanzador (16x16)
		string[] cactus = {
			"......KKKK......",
			".....KFFFFK.....",
			"......KFFK......",
			".....KKKKKK.....",
			"....KCCCCCCK....",
			"....KCWECWEK....",
			".KK.KCCCCCCK.KK.",
			"KCCKKCKKKKCKKCCK",
			"KCCCKCCCCCCKCCCK",
			".KCCKCcCCcCKCCK.",
			"..KKKCCCCCCKKK..",
			"....KCcCCcCK....",
			"....KCCCCCCK....",
			"...KTTTTTTTTK...",
			"....KTTTTTTK....",
			"....KKKKKKKK....",
		};
		var cactus2 = (string[])cactus.Clone();
		cactus2[1] = ".....KFyyFK.....";
		var disparo = (string[])cactus.Clone();
		disparo[7] = "KCCKKCKrrKCKKCCK";
		disparo[8] = "KCCCKCKrrKCKCCCK";
		disparo[1] = ".....KFyyFK.....";
		sCactus = new[] { Pixel("Cactus_1", cactus), Pixel("Cactus_2", cactus2) };
		sCactusDisparo = Pixel("Cactus_Disparo", disparo);

		//Espina del cactus
		sEspina = Pixel("Espina", new[] {
			"..KK..",
			".KcCK.",
			"KcCCCK",
			"KCCCCK",
			".KCCK.",
			"..KK..",
		});

		//Coco (12x12)
		Color cocoA = Hex("7a4a24"), cocoB = Hex("5a3418"), cocoC = Hex("a06a3a");
		sCoco = Proc("Coco", 12, 12, (x, y) => {
			if (!Elipse(x + 0.5f, y + 0.5f, 6f, 6f, 5.2f, 5.2f)) return Color.clear;
			if ((x == 4 || x == 7) && y == 7 || (x == 5 || x == 6) && y == 5) return Hex("2a1a10");
			if (Elipse(x + 0.5f, y + 0.5f, 4.3f, 7.8f, 1.6f, 1.4f)) return cocoC;
			return Hash(x, y, 3) > 0.7f ? cocoB : cocoA;
		}, true);

		//Estalactita (10x16) que cae del techo de la cueva
		sEstalactita = Proc("Estalactita", 10, 16, (x, y) => {
			float ancho = 4.5f * (y / 15f);
			if (Mathf.Abs(x + 0.5f - 5f) > ancho) return Color.clear;
			float luz = (x + 0.5f - 5f) / Mathf.Max(ancho, 0.5f);
			return Mezcla(Hex("a9a3c4"), Hex("5d5578"), luz * 0.5f + 0.5f);
		}, true);

		//Piedra que cae en la pelea del jefe
		sPiedra = Proc("Piedra", 14, 14, (x, y) => {
			if (!Elipse(x + 0.5f, y + 0.5f, 7f, 7f, 6.3f, 5.6f)) return Color.clear;
			return Mezcla(Hex("8d8396"), Hex("4e4658"), (14 - x + y) / 28f * 0.3f + (Hash(x, y, 9) > 0.8f ? 0.5f : 0.2f));
		}, true);

		//Púas (16x16, ocupan la mitad de abajo)
		sPuas = Proc("Puas", 16, 16, (x, y) => {
			if (y < 2) return Hex("4b4b55");
			int local = x % 4;
			float alto = 2f + (local < 2 ? local + 1 : 4 - local) * 2.6f;
			if (y > alto + 1) return Color.clear;
			return local < 2 ? Hex("e0e4ee") : Hex("9aa0b0");
		}, true);

		//Bola de fuego del jefe (4 frames)
		sFuego = new Sprite[4];
		for (int i = 0; i < 4; i++) {
			int k = i;
			sFuego[i] = Proc("Fuego_" + i, 14, 14, (x, y) => {
				float dx = x + 0.5f - 7f, dy = y + 0.5f - 7f;
				float ang = Mathf.Atan2(dy, dx);
				float r = Mathf.Sqrt(dx * dx + dy * dy) / (5.6f + Mathf.Sin(ang * 5f + k * 1.6f) * 0.9f);
				if (r > 1f) return Color.clear;
				if (r < 0.4f) return Hex("fff3b0");
				if (r < 0.7f) return Hex("ffb02e");
				return Hex("e8401c");
			}, false);
		}
	}

	// ------------------------------------------------ El Galipote (32x32)

	static void CrearJefeArte() {

		Color pelo = Hex("2b2035"), peloClaro = Hex("4a3a5c"), peloOscuro = Hex("1a1222");
		Color cuerno = Hex("eadfc4"), cuernoSombra = Hex("b3a684");
		Color ojo = Hex("ffd21f"), boca = Hex("8c1010"), diente = Hex("ffffff"), brillo = Hex("ff7a1a");

		Func<int, int, Color> Galipote(int paso, bool ataque, bool herido) {
			return (x, y) => {
				float fx = x + 0.5f, fy = y + 0.5f;
				//Cuernos curvos
				for (int lado = -1; lado <= 1; lado += 2) {
					float baseX = 16f + lado * 7.5f;
					for (float t = 0f; t <= 1f; t += 0.05f) {
						float cx = baseX + lado * (t * 5.5f);
						float cy = 23f + t * 7f - t * t * 2.5f;
						float grosor = 2.2f * (1f - t * 0.75f);
						if ((fx - cx) * (fx - cx) + (fy - cy) * (fy - cy) < grosor * grosor) return t > 0.6f ? cuernoSombra : cuerno;
					}
				}
				//Patas
				int alzar1 = paso == 1 ? 1 : 0, alzar2 = paso == 2 ? 1 : 0;
				if (y >= alzar1 && y < 5 + alzar1 && x >= 8 && x <= 12) return y == alzar1 && (x == 8 || x == 10 || x == 12) ? cuerno : peloOscuro;
				if (y >= alzar2 && y < 5 + alzar2 && x >= 19 && x <= 23) return y == alzar2 && (x == 19 || x == 21 || x == 23) ? cuerno : peloOscuro;
				//Cuerpo peludo con borde irregular
				float borde = 1f + Mathf.Sin(Mathf.Atan2(fy - 14f, fx - 16f) * 9f) * 0.06f;
				if (!Elipse(fx, fy, 16f, 14f, 12.5f * borde, 10.5f * borde)) return Color.clear;
				//Ojos
				for (int lado = -1; lado <= 1; lado += 2) {
					float ex = 16f + lado * 5f;
					if (Elipse(fx, fy, ex, 17f, 2.6f, 2.1f)) {
						if (Mathf.Abs(fx - ex) < 0.6f) return Hex("1a0a00");
						return herido ? Color.white : ojo;
					}
					//Cejas enojadas
					float cejaY = 20f + lado * (fx - ex) * 0.45f;
					if (Mathf.Abs(fx - ex) < 3f && Mathf.Abs(fy - cejaY) < 0.7f) return peloOscuro;
				}
				//Boca
				float altoBoca = ataque ? 5f : 2.6f;
				if (fx > 10f && fx < 22f && fy > 9.5f - altoBoca / 2f && fy < 9.5f + altoBoca / 2f) {
					float bordeSup = 9.5f + altoBoca / 2f, bordeInf = 9.5f - altoBoca / 2f;
					int col = Mathf.FloorToInt(fx) % 3;
					if (fy > bordeSup - 1.2f && col != 0) return diente;
					if (fy < bordeInf + 1.2f && col == 1) return diente;
					return ataque ? Mezcla(boca, brillo, 1f - Mathf.Abs(fy - 9.5f) / 2.5f) : boca;
				}
				//Sombreado del pelo
				float luz = ((fx - 16f) * -0.6f + (fy - 14f)) / 14f;
				if (Hash(x, y, 77) > 0.86f) return peloOscuro;
				return luz > 0.35f ? peloClaro : luz < -0.5f ? peloOscuro : pelo;
			};
		}

		sJefe = new[] {
			Proc("Galipote_1", 32, 32, Galipote(1, false, false), true),
			Proc("Galipote_2", 32, 32, Galipote(2, false, false), true),
		};
		sJefeAtaque = Proc("Galipote_Ataque", 32, 32, Galipote(0, true, false), true);
		sJefeHerido = Proc("Galipote_Herido", 32, 32, Galipote(0, true, true), true);
	}

	// ------------------------------------------------ Objetos y bloques

	static void CrearObjetos() {

		//Larimar: piedra azul con vetas blancas
		Color larA = Hex("4fc3e8"), larB = Hex("1f8fc4"), larC = Hex("c8f2ff");
		Func<int, int, Color> FormaLarimar(float escalaX) => (x, y) => {
			float dx = (x + 0.5f - 6f) / escalaX, dy = y + 0.5f - 6f;
			if (Mathf.Abs(dx) + Mathf.Abs(dy) * 0.8f > 5.6f) return Color.clear;
			float veta = Mathf.Sin(dx * 1.3f + dy * 0.8f) + Mathf.Sin(dy * 2.1f - dx * 0.5f);
			if (veta > 1.45f) return larC;
			return dx - dy > 1f ? larB : larA;
		};
		sLarimar = Proc("Larimar", 12, 12, FormaLarimar(1f), true);
		sLarimarGira = new[] {
			sLarimar,
			Proc("Larimar_Gira1", 12, 12, FormaLarimar(0.6f), true),
			Proc("Larimar_Gira2", 12, 12, FormaLarimar(0.25f), true),
			Proc("Larimar_Gira3", 12, 12, FormaLarimar(0.6f), true),
		};

		//Mango maduro con hoja
		sMango = Proc("Mango", 14, 14, (x, y) => {
			float fx = x + 0.5f, fy = y + 0.5f;
			if (fy > 10.5f && fx > 6.5f && fx < 11.5f && Mathf.Abs(fy - (11.5f + (fx - 7f) * 0.4f)) < 1.1f) return Hex("3f9a2c");
			if (fy > 10f && Mathf.Abs(fx - 6.5f) < 0.6f) return Hex("6b4a1a");
			if (!Elipse(fx, fy, 7f, 6f, 5.5f, 4.8f)) return Color.clear;
			if (Elipse(fx, fy, 5f, 7.5f, 1.4f, 1f)) return Hex("fff1a8");
			float t = (fx - fy + 6f) / 14f;
			return Mezcla(Hex("ffcc33"), Hex("ff5a2a"), t);
		}, true);

		//Bloque sorpresa (dorado con signo de pregunta), 2 frames
		string[] signo = {
			".KKK.",
			"K...K",
			"....K",
			"...K.",
			"..K..",
			".....",
			"..K..",
		};
		Func<int, int, Color> Sorpresa(float brilloExtra) => (x, y) => {
			if (x == 0 || y == 0 || x == 15 || y == 15) return CONTORNO;
			int sx = x - 5, sy = 11 - y;
			if (sx >= 0 && sx < 5 && sy >= 0 && sy < 7 && signo[sy][sx] == 'K') return Hex("7a3e10");
			if (sx >= 1 && sx < 6 && sy >= 1 && sy < 8 && signo[sy - 1][sx - 1] == 'K') return Hex("f8e7b0");
			if ((x == 2 || x == 13) && (y == 2 || y == 13)) return Hex("7a3e10");
			if (x == 1 || y == 14) return Mezcla(Hex("fff0a0"), Color.white, brilloExtra);
			if (x == 14 || y == 1) return Hex("c4740e");
			return Mezcla(Hex("f5b324"), Hex("ffd75a"), brilloExtra);
		};
		sSorpresa = new[] { Proc("Bloque_Sorpresa1", 16, 16, Sorpresa(0f)), Proc("Bloque_Sorpresa2", 16, 16, Sorpresa(0.45f)) };

		sVacio = Proc("Bloque_Vacio", 16, 16, (x, y) => {
			if (x == 0 || y == 0 || x == 15 || y == 15) return CONTORNO;
			if ((x == 2 || x == 13) && (y == 2 || y == 13)) return Hex("3e2614");
			if (x == 1 || y == 14) return Hex("a8794e");
			if (x == 14 || y == 1) return Hex("5a3a20");
			return Hex("8a5e38");
		});

		sEscombro = Proc("Escombro", 6, 6, (x, y) => (x + y) % 5 == 0 ? Hex("6b2f14") : Hex("c8642e"), true);

		//Plataforma de madera (atravesable desde abajo) y plataforma que se cae
		sPlataforma = Proc("Plataforma", 16, 8, (x, y) => {
			if (y < 2) return Color.clear;
			if (y == 2 || y == 7) return CONTORNO;
			if (x % 8 == 0) return Hex("5a3418");
			if (y == 6) return Hex("d8a060");
			return Hash(x / 2, y, 5) > 0.75f ? Hex("8a5428") : Hex("b07a40");
		}, false, null, true);
		sPlataformaCae = Proc("Plataforma_Cae", 16, 8, (x, y) => {
			if (y < 2) return Color.clear;
			if (y == 2 || y == 7 || x == 0 || x == 15) return CONTORNO;
			if ((x == 5 && y > 3) || (x == 6 && y == 4) || (x == 11 && y < 6) || (x == 10 && y == 4)) return Hex("3a1a0a");
			if (y == 6) return Hex("f08060");
			return Hex("c0503a");
		});

		//Efectos: brillo, polvo y explosión
		sBrillo = new Sprite[4];
		for (int i = 0; i < 4; i++) {
			float r = 2f + i * 1.6f;
			int k = i;
			sBrillo[i] = Proc("Brillo_" + i, 16, 16, (x, y) => {
				float dx = Mathf.Abs(x + 0.5f - 8f), dy = Mathf.Abs(y + 0.5f - 8f);
				bool cruz = (dx < 0.6f && dy < r) || (dy < 0.6f && dx < r) || (k < 2 && Mathf.Abs(dx - dy) < 0.5f && dx < r * 0.6f);
				return cruz ? (k == 3 ? Hex("a0e8ff") : Color.white) : Color.clear;
			});
		}
		sPolvo = new Sprite[4];
		for (int i = 0; i < 4; i++) {
			int k = i;
			sPolvo[i] = Proc("Polvo_" + i, 16, 16, (x, y) => {
				for (int b = 0; b < 3; b++) {
					float ang = b * 2.1f + k * 0.5f;
					float cx = 8f + Mathf.Cos(ang) * (2f + k * 1.5f), cy = 7f + Mathf.Sin(ang) * (1.5f + k);
					if (Elipse(x + 0.5f, y + 0.5f, cx, cy, 3.2f - k * 0.7f, 3.2f - k * 0.7f)) return k < 2 ? Hex("f4f0e8") : Hex("c8c0b4");
				}
				return Color.clear;
			});
		}
		sExplosion = new Sprite[5];
		for (int i = 0; i < 5; i++) {
			int k = i;
			sExplosion[i] = Proc("Explosion_" + i, 24, 24, (x, y) => {
				float d = Vector2.Distance(new Vector2(x + 0.5f, y + 0.5f), new Vector2(12f, 12f));
				float radio = 4f + k * 2f;
				float ruido = Hash(x / 2, y / 2, k) * 2.5f;
				if (d > radio + ruido) return Color.clear;
				if (k >= 3 && d < radio - 3f) return Color.clear;
				float t = d / (radio + 2.5f);
				return k >= 3 ? Hex("7a6a6a") : t < 0.4f ? Hex("fff6c0") : t < 0.7f ? Hex("ffa424") : Hex("e0401a");
			});
		}

		//Meta: poste, bandera dominicana y bandera del punto de control
		sPoste = Proc("Poste", 16, 16, (x, y) => x >= 7 && x <= 8 ? (x == 7 ? Hex("e8e8e8") : Hex("9a9aa8")) : Color.clear, false, null, true);
		sBanderaMeta = Proc("Bandera_Meta", 24, 16, (x, y) => {
			if (x == 0 || y == 0 || x == 23 || y == 15) return CONTORNO;
			bool cruzX = x >= 10 && x <= 13, cruzY = y >= 7 && y <= 8;
			if (cruzX || cruzY) {
				if (x >= 10 && x <= 13 && y >= 6 && y <= 9) return (x + y) % 3 == 0 ? Hex("2e8b3a") : Hex("1a4fa0");
				return Color.white;
			}
			bool arriba = y > 8, izquierda = x < 10;
			return (arriba == izquierda) ? Hex("1a3f9a") : Hex("d0202c");
		}, false, new Vector2(0f, 0.5f));
		Func<Color, Color, Func<int, int, Color>> Bandera = (a, b) => (x, y) => {
			if (x <= 1) return x == 0 ? Hex("e8e8e8") : Hex("9a9aa8");
			if (y < 8) return Color.clear;
			float largo = 2f + (y - 8) * 1.4f;
			if (x > largo + 2f) return Color.clear;
			return (x + y) % 4 == 0 ? b : a;
		};
		sPuntoControl = Proc("PuntoControl", 14, 18, Bandera(Hex("8a8a96"), Hex("6a6a76")), true, new Vector2(0.15f, 0f));
		sPuntoControlActivo = Proc("PuntoControl_Activo", 14, 18, Bandera(Hex("ffd23a"), Hex("4fc3e8")), true, new Vector2(0.15f, 0f));
	}

	// ------------------------------------------------ Escenarios: tiles y fondos de cada nivel

	static void CrearTemas() {
		temas = new Tema[3];

		//Nivel 1: Playa de Samaná (de día)
		temas[0] = CrearTema("Playa", Hex("5cbf3a"), Hex("3e9a2a"), Hex("e8c07a"), Hex("c99a55"), Hex("fff0c8"),
			Hex("c8642e"), Hex("6b2f14"), Hex("b9a48a"), Hex("4aa8f0"), Hex("c6ecff"), Hex("2a9fd6"));
		temas[0].lejano = Proc("Playa_Lejano", 256, 96, (x, y) => {
			//Lomas de Samaná en el horizonte y el mar abajo
			float loma = 38f + Mathf.Sin(x * 0.0245f) * 14f + Mathf.Sin(x * 0.0736f + 1f) * 6f;
			if (y < 22) return (y + x / 7) % 9 == 0 && Hash(x, y, 1) > 0.5f ? Hex("bff0ff") : Mezcla(Hex("3cb4e6"), Hex("1f86c4"), (22 - y) / 22f);
			if (y < loma) return Mezcla(Hex("6fb7a8"), Hex("8fcabb"), (y - 22f) / 40f);
			return Color.clear;
		}, false, new Vector2(0.5f, 0f), true);
		temas[0].medio = Proc("Playa_Medio", 256, 64, (x, y) => {
			float colina = 18f + Mathf.Sin(x * 0.049f) * 7f + Mathf.Sin(x * 0.11f) * 3f;
			if (y < colina) return Hash(x, y, 2) > 0.92f ? Hex("4c9a3a") : Hex("5aac46");
			return Color.clear;
		}, false, new Vector2(0.5f, 0f), true);

		//Nivel 2: Cueva de las Maravillas
		temas[1] = CrearTema("Cueva", Hex("9c8cc0"), Hex("7a6aa0"), Hex("4b3f63"), Hex("3a3050"), Hex("6fd3ff"),
			Hex("6a5a8a"), Hex("2a2040"), Hex("8a84a0"), Hex("120c1f"), Hex("2e2245"), Hex("2b6cb0"));
		temas[1].lejano = Proc("Cueva_Lejano", 256, 240, (x, y) => {
			float suelo = 40f + Mathf.Sin(x * 0.037f) * 18f;
			float techo = 200f - Mathf.Abs(Mathf.Sin(x * 0.09f)) * 50f - Mathf.Sin(x * 0.023f) * 15f;
			if (y < suelo || y > techo) return Hex("1d1530");
			if (Hash(x / 3, y / 3, 4) > 0.985f) return Hex("3d6a9a");
			return Color.clear;
		}, false, new Vector2(0.5f, 0f), true);
		temas[1].medio = Proc("Cueva_Medio", 256, 240, (x, y) => {
			//Columnas de piedra y estalactitas
			float c = Mathf.Abs(((x + 40) % 128) - 64f);
			if (c < 10f + Mathf.Sin(y * 0.08f) * 3f) return Hash(x, y, 5) > 0.9f ? Hex("3a2e55") : Hex("2e2445");
			int estal = x % 37;
			float largo = 30f + Hash(x / 37, 0, 6) * 40f;
			if (y > 240 - largo * (1f - Mathf.Abs(estal - 18f) / 18f) && estal < 36) return Hex("2a2140");
			return Color.clear;
		}, false, new Vector2(0.5f, 0f), true);

		//Nivel 3: Loma del Galipote (de noche)
		temas[2] = CrearTema("Noche", Hex("3f8a4a"), Hex("2a6036"), Hex("4a3426"), Hex("382619"), Hex("6a5040"),
			Hex("6a4a5a"), Hex("2a1a24"), Hex("7a7088"), Hex("0b1030"), Hex("4a2f62"), Hex("234a6a"));
		temas[2].lejano = Proc("Noche_Lejano", 256, 96, (x, y) => {
			float monte = 50f + Mathf.Sin(x * 0.0245f) * 20f + Mathf.Sin(x * 0.098f) * 6f;
			if (y < monte) return Mezcla(Hex("1e1a3a"), Hex("2b2450"), y / monte);
			return Color.clear;
		}, false, new Vector2(0.5f, 0f), true);
		temas[2].medio = Proc("Noche_Medio", 256, 80, (x, y) => {
			float colina = 22f + Mathf.Sin(x * 0.049f + 2f) * 8f;
			if (y < colina) return Hex("141228");
			//Siluetas de árboles
			int arbol = x % 64;
			if (arbol > 20 && arbol < 44) {
				float cx = 32f;
				if (Mathf.Abs(arbol - cx) < 1.5f && y < colina + 22f) return Hex("141228");
				if (Elipse(arbol, y, cx, colina + 28f, 11f, 12f) && Hash(x / 2, y / 2, 7) > 0.15f) return Hex("141228");
			}
			return Color.clear;
		}, false, new Vector2(0.5f, 0f), true);
	}

	static Tema CrearTema(string n, Color pasto, Color pastoOscuro, Color tierra, Color tierraOscura, Color mota,
		Color ladrillo, Color mortero, Color piedra, Color cieloArriba, Color cieloAbajo, Color agua) {

		var t = new Tema { nombre = n, colorAgua = agua };
		int semilla = n.Length * 13;

		t.superficie = Proc(n + "_Superficie", 16, 16, (x, y) => {
			int borde = 11 - (int)(Hash(x, 0, semilla) * 2.5f);
			if (y >= 15) return Hash(x, 1, semilla) > 0.35f ? pastoOscuro : Color.clear;
			if (y > borde) return y == 14 ? Mezcla(pasto, Color.white, 0.25f) : (Hash(x, y, semilla) > 0.8f ? pastoOscuro : pasto);
			if (y == borde) return pastoOscuro;
			float hsh = Hash(x, y, semilla + 1);
			return hsh > 0.93f ? mota : hsh > 0.75f ? tierraOscura : tierra;
		}, false, null, true);
		t.interior = Proc(n + "_Interior", 16, 16, (x, y) => {
			float hsh = Hash(x, y, semilla + 2);
			return hsh > 0.95f ? mota : hsh > 0.75f ? tierraOscura : tierra;
		}, false, null, true);
		t.piedra = Proc(n + "_Piedra", 16, 16, (x, y) => {
			if (x == 0 || y == 0 || x == 15 || y == 15) return CONTORNO;
			if (x == 1 || y == 14) return Mezcla(piedra, Color.white, 0.35f);
			if (x == 14 || y == 1) return Mezcla(piedra, Color.black, 0.35f);
			if (x + y == 15 || x + y == 16) return Mezcla(piedra, Color.black, 0.15f);
			return piedra;
		});
		t.ladrillo = Proc(n + "_Ladrillo", 16, 16, (x, y) => {
			if (y % 4 == 0) return mortero;
			int fila = y / 4;
			if ((x + (fila % 2) * 4) % 8 == 0) return mortero;
			if (y % 4 == 3) return Mezcla(ladrillo, Color.white, 0.3f);
			return ladrillo;
		});
		t.cielo = Proc(n + "_Cielo", 4, 64, (x, y) => Mezcla(cieloAbajo, cieloArriba, y / 63f));

		//Agua animada (2 frames)
		t.agua = new Sprite[2];
		for (int i = 0; i < 2; i++) {
			int k = i;
			t.agua[i] = Proc(n + "_Agua" + i, 16, 16, (x, y) => {
				float ola = 13f + Mathf.Sin((x + k * 4) * 0.785f) * 1.2f;
				if (y > ola + 1f) return Color.clear;
				if (y > ola) return new Color(1f, 1f, 1f, 0.85f);
				var c = Mezcla(agua, Color.black, (14 - y) / 30f);
				c.a = 0.85f;
				return c;
			}, false, null, true);
		}
		return t;
	}

	static void CrearDecoracion() {

		//Palmera (32x80)
		sPalmera = Proc("Palmera", 32, 80, (x, y) => {
			float fx = x + 0.5f, fy = y + 0.5f;
			float troncoX = 16f + Mathf.Sin(fy * 0.035f) * 3f;
			//Hojas
			float[] angulos = { -0.35f, 0.25f, 0.9f, 1.57f, 2.24f, 2.9f, 3.5f };
			for (int h = 0; h < angulos.Length; h++) {
				float ang = angulos[h];
				float dirX = Mathf.Cos(ang), dirY = Mathf.Sin(ang);
				float largo = Mathf.Abs(dirX) > 0.5f ? 15f : 10f;
				for (float t = 0.05f; t < 1f; t += 0.03f) {
					float px = troncoX + dirX * t * largo, py = 68f + dirY * t * 9f - t * t * (Mathf.Abs(dirX) * 13f);
					float g = 2.4f * (1f - t * 0.55f);
					if ((fx - px) * (fx - px) + (fy - py) * (fy - py) < g * g) return (h % 2 == 0) ? Hex("2f8a2a") : Hex("48b03a");
				}
			}
			//Cocos
			if (Elipse(fx, fy, troncoX - 2f, 62f, 2f, 2f) || Elipse(fx, fy, troncoX + 2f, 62f, 2f, 2f)) return Hex("6a3e1c");
			if (fy < 66f && Mathf.Abs(fx - troncoX) < 2.2f) return (y % 5 == 0) ? Hex("6a4a2a") : Hex("9a7448");
			return Color.clear;
		}, true, new Vector2(0.5f, 0f));

		sArbusto = Proc("Arbusto", 32, 16, (x, y) => {
			float fx = x + 0.5f, fy = y + 0.5f;
			if (Elipse(fx, fy, 9f, 4f, 8f, 8f) || Elipse(fx, fy, 17f, 6f, 8f, 9f) || Elipse(fx, fy, 25f, 4f, 7f, 7f)) {
				if (Hash(x, y, 8) > 0.93f) return Hex("ff6fa8");
				return fy > 10f ? Hex("5cc04a") : Hex("3e9a2a");
			}
			return Color.clear;
		}, true, new Vector2(0.5f, 0f));

		//Casitas de colores típicas de Samaná
		Func<Color, Color, Func<int, int, Color>> Casita = (pared, techo) => (x, y) => {
			float fx = x + 0.5f, fy = y + 0.5f;
			if (fy > 26f) {
				float ancho = 24f - (fy - 26f) * 1.6f;
				if (Mathf.Abs(fx - 24f) < ancho) return (y % 3 == 0) ? Mezcla(techo, Color.black, 0.2f) : techo;
				return Color.clear;
			}
			if (fx < 4f || fx > 44f) return Color.clear;
			if (fx > 18f && fx < 27f && fy < 18f) return Hex("6a3e1c");
			if ((fx > 7f && fx < 14f || fx > 32f && fx < 39f) && fy > 10f && fy < 19f) return (fx > 10f && fx < 11f || fx > 35f && fx < 36f) ? Hex("6a3e1c") : Hex("2a3a5a");
			return (x % 4 == 0) ? Mezcla(pared, Color.black, 0.12f) : pared;
		};
		sCasita1 = Proc("Casita_1", 48, 40, Casita(Hex("f2a65a"), Hex("d84a3a")), true, new Vector2(0.5f, 0f));
		sCasita2 = Proc("Casita_2", 48, 40, Casita(Hex("7ad0c8"), Hex("3a6ac8")), true, new Vector2(0.5f, 0f));

		sNube = Proc("Nube", 48, 20, (x, y) => {
			float fx = x + 0.5f, fy = y + 0.5f;
			if (Elipse(fx, fy, 14f, 7f, 10f, 6f) || Elipse(fx, fy, 26f, 11f, 12f, 8f) || Elipse(fx, fy, 37f, 7f, 9f, 6f)) return fy < 6f ? Hex("dceef8") : Color.white;
			return Color.clear;
		});

		//Cueva
		sCristal = Proc("Cristal", 24, 24, (x, y) => {
			float fx = x + 0.5f, fy = y + 0.5f;
			for (int c = 0; c < 3; c++) {
				float cx = 6f + c * 6f, alto = c == 1 ? 22f : 14f;
				float ancho = 3f * (1f - Mathf.Max(0f, fy - alto + 6f) / 6f);
				if (fy < alto && Mathf.Abs(fx - cx) < ancho) return fx < cx ? Hex("9af0ff") : Hex("3ab6e0");
			}
			return Color.clear;
		}, true, new Vector2(0.5f, 0f));
		sHongo = Proc("Hongo", 16, 16, (x, y) => {
			float fx = x + 0.5f, fy = y + 0.5f;
			if (fy > 6f && Elipse(fx, fy, 8f, 7f, 7f, 6f)) return Hash(x, y, 12) > 0.8f ? Hex("e8ffb0") : Hex("8ae04a");
			if (fy <= 7f && Mathf.Abs(fx - 8f) < 2f) return Hex("e8dcc0");
			return Color.clear;
		}, true, new Vector2(0.5f, 0f));
		sEstalagmita = Proc("Estalagmita", 16, 32, (x, y) => {
			float ancho = 7f * (1f - y / 32f);
			if (Mathf.Abs(x + 0.5f - 8f) > ancho) return Color.clear;
			return x < 8 ? Hex("7a6aa0") : Hex("564878");
		}, true, new Vector2(0.5f, 0f));

		//Noche
		sArbolSeco = Proc("ArbolSeco", 40, 64, (x, y) => {
			float fx = x + 0.5f, fy = y + 0.5f;
			if (Mathf.Abs(fx - 20f - Mathf.Sin(fy * 0.08f) * 1.5f) < 2.5f - fy / 64f && fy < 50f) return Hex("3a2a2a");
			for (int r = 0; r < 4; r++) {
				float baseY = 24f + r * 8f, lado = r % 2 == 0 ? 1f : -1f;
				for (float t = 0f; t < 1f; t += 0.05f) {
					float px = 20f + lado * t * 14f, py = baseY + t * 10f + t * t * 4f;
					if ((fx - px) * (fx - px) + (fy - py) * (fy - py) < 1.4f) return Hex("3a2a2a");
				}
			}
			return Color.clear;
		}, true, new Vector2(0.5f, 0f));
		sLuna = Proc("Luna", 48, 48, (x, y) => {
			float fx = x + 0.5f, fy = y + 0.5f;
			if (!Elipse(fx, fy, 24f, 24f, 20f, 20f)) return Elipse(fx, fy, 24f, 24f, 23f, 23f) ? new Color(1f, 0.95f, 0.8f, 0.2f) : Color.clear;
			if (Elipse(fx, fy, 17f, 28f, 4f, 4f) || Elipse(fx, fy, 29f, 17f, 5f, 4f) || Elipse(fx, fy, 30f, 31f, 2.5f, 2.5f)) return Hex("e0d8b8");
			return Hex("fff6d8");
		});
		sEstrellas = Proc("Estrellas", 128, 128, (x, y) => {
			float h = Hash(x, y, 31);
			if (h > 0.996f) return Color.white;
			if (h > 0.990f) return new Color(1f, 1f, 1f, 0.45f);
			return Color.clear;
		}, false, null, true);
		sCruz = Proc("Cruz", 12, 20, (x, y) => {
			if (x >= 5 && x <= 6 && y < 18) return Hex("6a5a4a");
			if (y >= 12 && y <= 13 && x >= 1 && x <= 10) return Hex("6a5a4a");
			return Color.clear;
		}, true, new Vector2(0.5f, 0f));
		sLuciernaga = new Sprite[2];
		for (int i = 0; i < 2; i++) {
			int k = i;
			sLuciernaga[i] = Proc("Luciernaga_" + i, 8, 8, (x, y) => {
				float d = Vector2.Distance(new Vector2(x + 0.5f, y + 0.5f), new Vector2(4f, 4f));
				if (d < 1.2f) return Hex("fbffb0");
				if (d < (k == 0 ? 3.5f : 2.5f)) return new Color(0.9f, 1f, 0.4f, 0.35f);
				return Color.clear;
			});
		}
	}

	// ------------------------------------------------ Interfaz

	static void CrearInterfazArte() {

		sCorazon = Pixel("Corazon", new[] {
			"................",
			"..KKKK....KKKK..",
			".KRRllK..KRRRRK.",
			"KRRlllRKKRRRRRRK",
			"KRRllRRRRRRRRRRK",
			"KRRRRRRRRRRRRRRK",
			"KRRRRRRRRRRRRRrK",
			".KRRRRRRRRRRRrK.",
			"..KRRRRRRRRRrK..",
			"...KRRRRRRRrK...",
			"....KRRRRRrK....",
			".....KRRRrK.....",
			"......KRrK......",
			".......KK.......",
			"................",
			"................",
		}, new Dictionary<char, Color> { { 'l', Hex("ff9a8a") } });

		//Panel y botón redondeados (9-slice)
		sPanel = Proc("UI_Panel", 32, 32, (x, y) => {
			float dx = Mathf.Max(0f, Mathf.Abs(x + 0.5f - 16f) - 10f), dy = Mathf.Max(0f, Mathf.Abs(y + 0.5f - 16f) - 10f);
			float d = Mathf.Sqrt(dx * dx + dy * dy);
			if (d > 6f) return Color.clear;
			if (d > 4.5f) return new Color(1f, 0.85f, 0.35f, 1f);
			return new Color(0.08f, 0.06f, 0.15f, 0.88f);
		}, false, null, false, new Vector4(12, 12, 12, 12));
		sBoton = Proc("UI_Boton", 32, 32, (x, y) => {
			float dx = Mathf.Max(0f, Mathf.Abs(x + 0.5f - 16f) - 10f), dy = Mathf.Max(0f, Mathf.Abs(y + 0.5f - 16f) - 10f);
			float d = Mathf.Sqrt(dx * dx + dy * dy);
			if (d > 6f) return Color.clear;
			if (d > 4.8f) return CONTORNO;
			if (y < 8) return Color.Lerp(Color.white, Color.gray, 0.35f);
			return Color.white;
		}, false, null, false, new Vector4(12, 12, 12, 12));
		sBlanco = Proc("UI_Blanco", 4, 4, (x, y) => Color.white);
	}
}
