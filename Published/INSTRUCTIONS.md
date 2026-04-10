# FeladatRadar – Telepítési és futtatási útmutató

## Élő alkalmazás

Az alkalmazás elérhető az alábbi címen:

**[https://feladatradar.hu/](https://feladatradar.hu/)**

---

## Felhasznált szoftverek

A projekt fejlesztése során az alábbi szoftvereket és eszközöket alkalmaztuk. Minden eszközt a szerepének megfelelően választottunk ki, hogy a fejlesztési folyamat hatékony és áttekinthető legyen.

| Szoftver | Verzió | Szerep a projektben |
|---|---|---|
| Microsoft Visual Studio 2022 | 17.x | Fő IDE, backend és frontend fejlesztés |
| SQL Server 2022 (Developer) | 16.x | Adatbázis-szerver |
| SQL Server Management Studio 21 | 21.x | Adatbázis-kezelés, tárolt eljárás írás és tesztelés |
| GitHub | 2.51.0 | Közös kódtároló, branch-alapú munka |
| .NET SDK | net8.0 | Build és futtatókörnyezet |

---

## Lokális futtatás lépései

### 1. Repository klónozása

Nyisson egy terminált (pl. PowerShell vagy Git Bash), navigáljon a kívánt mappába, majd futtassa:

```bash
git clone https://github.com/RobiGanz/FeladatRadar.git
```

A parancs létrehozza a `FeladatRadar` nevű mappát a gépén, és letölti a teljes forráskódot. Ha nincs Git telepítve, előbb töltse le a [git-scm.com](https://git-scm.com/downloads) oldalról.

### 2. Megoldás megnyitása Visual Studio-ban

Nyissa meg a `FeladatRadar.sln` fájlt Visual Studio 2022-ben. Ha a NuGet-csomagok visszaállítása nem indul el automatikusan:

> Jobb klikk a Solution-ön → **Restore NuGet Packages**

### 3. Adatbázis inicializálása

1. Nyissa meg az SQL Server Management Studio-t, és csatlakozzon a helyi SQL Server példányhoz
2. **File → Open → File** menüponttal nyissa meg az `ures.sql` fájlt (`FeladatRadar\database\dump\ures.sql`)
3. Futtassa **F5**-tel – ez létrehozza az adatbázist, az összes táblát és a tárolt eljárásokat
4. Amennyiben tesztadatokra is szükség van, futtassa a `tablafeltoltes.sql` szkriptet is, amely feltölti az adatbázist adatokkal
5. Ellenőrzés: a `Users` táblában meg kell jelennie a tesztfelhasználóknak

> **Alternatíva:** A `FeladatRadar.bacpac` fájl visszaállítható SSMS-ben: jobb klikk a **Databases**-en → **Import Data-tier Application...** → válassza ki a `.bacpac` fájlt

### 4. appsettings.json beállítása (backend)

A `FeladatRadar.backend/appsettings.json` fájlban cserélje le a `ConnectionStrings > Default` értékét a lokális kapcsolatra:

**SQL Server Authentication:**
```json
"Default": "Server=GEPNEV\\SQLEXPRESS;Database=FeladatRadar;user=sa;password=JELSZO;TrustServerCertificate=true"
```

**Windows Authentication:**
```json
"Default": "Server=.\\SQLEXPRESS;Database=FeladatRadar;Integrated Security=true;TrustServerCertificate=true"
```

> A gépnevet az SSMS kapcsolódási ablakában a **Server name** mezőben láthatja.

### 5. Multiple Startup Projects beállítása

Hogy egyszerre induljon el a backend és a frontend:

> Jobb klikk a Solution-ön → **Set Startup Projects...** → **Multiple startup projects** → mindkét projektnél (`FeladatRadar.backend` és `FeladatRadar.frontend`) az Action értéke legyen **Start** → OK

Ezt csak egyszer kell beállítani, a Visual Studio elmenti.

### 6. Futtatás

Nyomjon **F5**-öt, vagy kattintson a zöld **Start** gombra.

| Szolgáltatás | Cím |
|---|---|
| Backend (Swagger) | https://localhost:44359/swagger |
| Frontend | https://localhost:7189/ |

> Ha a böngésző SSL-figyelmeztetést jelez, fogadja el a fejlesztői tanúsítványt: **Advanced → Proceed**

---

## Fejlesztői tesztfelhasználók

| Szerepkör | Felhasználónév | Jelszó | Leírás |
|---|---|---|---|
| Student (Diák) | tesztdiak | Teszt123! | Feladatok, órarend, csoportok |
| Teacher (Tanár) | tesztanar | Teszt123! | Dolgozat hozzáadása, tanári csoportkezelés |
| Admin | tesztadmin | Teszt123! | Rendszerkezelés, felhasználókezelés, audit napló |

> A jelszavak BCrypt hash-ként tárolódnak az adatbázis `PasswordHash` mezőjében.
