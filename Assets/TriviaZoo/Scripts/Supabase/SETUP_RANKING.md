# Ranking con Supabase — Guía de configuración

## 1. Crear la tabla y las políticas (Supabase)

En el dashboard de Supabase: **SQL Editor → New query**, pega esto y ejecútalo.

```sql
create table public.ranking (
  id            bigint generated always as identity primary key,
  player_name   text     not null,
  question_type smallint not null,
  category      smallint not null,
  score         integer  not null,
  created_at    timestamptz not null default now()
);

-- Índice para que el "top" (order by score desc por tipo+categoría) sea rápido
create index ranking_top_idx
  on public.ranking (question_type, category, score desc);

-- RLS activado
alter table public.ranking enable row level security;

-- Permitir lectura pública (el GET del top)
create policy "ranking_anon_select"
  on public.ranking for select
  to anon
  using (true);

-- Permitir inserción pública (el POST del puntaje)
create policy "ranking_anon_insert"
  on public.ranking for insert
  to anon
  with check (score >= 0 and char_length(player_name) between 1 and 40);
```

Nota: agregué la columna `category` además de `question_type` porque el top se muestra
"según la categoría" y debe coincidir con tu key local `score_{question_type}_{category}`.

## 2. Obtener la anon key

Dashboard → **Project Settings → API → Project API keys → `anon` `public`**.
Cópiala (es pública por diseño; la seguridad la da RLS).

## 3. Crear el asset de configuración en Unity

1. En el Project window: clic derecho → **Create → TriviaZoo → Supabase Config**.
2. Nómbralo **exactamente** `SupabaseConfig`.
3. Muévelo a una carpeta `Resources` (p. ej. `Assets/TriviaQuizKit/Resources/SupabaseConfig.asset`).
   Debe quedar dentro de algún `Resources/` para que `Resources.Load` lo encuentre.
4. En el Inspector:
   - **Url**: `https://eynyocldtcruwxaynrpo.supabase.co` (ya viene por defecto)
   - **Anon Key**: pega la clave del paso 2
   - **Table Name**: `ranking`

Si el asset falta o la key está vacía, el ranking se desactiva solo (sin romper el juego)
y verás un warning en consola.

## 4. Cambios en el prefab ProfilePopup (Unity Editor)

Abre `Assets/TriviaQuizKit/Resources/Popups/ProfilePopup.prefab` en modo prefab.

**a) Campo de nombre (zona del avatar):**
1. En la zona del avatar, clic derecho → **UI → Input Field - TextMeshPro**.
2. Ajústalo/posiciónalo donde quieras dentro de esa zona.
3. (Opcional) Pon un Placeholder tipo "Tu nombre".
4. Selecciona el GameObject raíz del prefab (el que tiene el componente `ProfilePopup`).
5. En el Inspector, arrastra el `TMP_InputField` al nuevo campo **Name Input**.

No necesitas cablear el evento `onEndEdit` a mano: el script lo registra solo.

## 5. Cambios en el prefab del item de categoría (top global)

El "top" se pinta dentro de cada item de categoría (el del Tigrillo).

1. Localiza el prefab del `CategoryScrollItem` (el `CategoryScrollItemPrefab` que usa
   `ProfilePopup`; revisa el campo en el Inspector del `ProfilePopup` para saber cuál es).
2. Ábrelo en modo prefab.
3. Debajo del Tigrillo / del high score personal, crea un **UI → Text - TextMeshPro**
   (multilínea, con alto suficiente para ~10 líneas).
4. Selecciona el GameObject raíz del item (el del componente `CategoryScrollItem`).
5. Arrastra ese texto al nuevo campo **Global Top Text**.

Al abrir el ProfilePopup, ese texto mostrará el top 10 (nombre + puntaje) de Supabase
para el tipo de pregunta seleccionado y esa categoría. Si no hay config, queda vacío;
si no hay registros, muestra "Sin datos".

## 6. Cómo se envía el puntaje

- Al terminar una partida, `GameScreen` detecta si hubo **nuevo high score** y se lo pasa
  al `GameFinishedPopup`.
- Al cerrar ese popup (Replay o Quit), si hubo nuevo high score, se hace el POST a Supabase
  con: `player_name`, `question_type`, `category`, `score`.
- El nombre se toma de PlayerPrefs `player_name`. Si está vacío, se genera uno por defecto
  tipo `Jugador#260628X` (Jugador + AAMMDD + dígito aleatorio) y se persiste.

## Notas

- Sin SDK: todo es `UnityWebRequest` + REST con la anon key en los headers `apikey` y
  `Authorization: Bearer`.
- El servicio (`SupabaseRankingService`) se auto-inicializa con `DontDestroyOnLoad`, así
  que el POST sobrevive aunque la escena cambie al salir al Home.
- `question_type`: 0=Single, 1=Multiple, 2=TrueFalse (el ProfilePopup solo cicla 0-2).
```
