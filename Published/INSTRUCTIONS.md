# FeladatRadar – Telepítési és futtatási útmutató

## Élő alkalmazás

Az alkalmazás elérhető az alábbi címen:

**[https://www.feladatradar.hu](https://www.feladatradar.hu)**

---

## Előfeltételek

| Szoftver | Verzió | Letöltés |
|---|---|---|
| SQL Server (Developer/Express) | 2019+ | https://www.microsoft.com/sql-server/sql-server-downloads |
| SQL Server Management Studio | 20+ | https://aka.ms/ssmsfullsetup |
| Visual Studio | 2022 (17.8+) | https://visualstudio.microsoft.com/vs/ |
| Git Bash | 2.51+ | https://git-scm.com/downloads |

---

## Lokális futtatás lépései

### 1. Repository klónozása

Nyisson egy terminált (PowerShell vagy Git Bash), navigáljon a kívánt mappába, majd futtassa:

```bash
git clone https://github.com/RobiGanz/FeladatRadar.git
```

### 2. Megoldás megnyitása Visual Studio-ban

Nyissa meg a `FeladatRadar.sln` fájlt Visual Studio 2022-ben. Ha a NuGet-csomagok visszaállítása nem indul el automatikusan:

> Jobb klikk a Solution-ön → **Restore NuGet Packages**

### 3. Adatbázis létrehozása SSMS-ben

1. Nyissa meg az SQL Server Management Studio-t, csatlakozzon a helyi SQL Server példányhoz
2. **File → Open → File** menüponttal nyissa meg az `ures.sql` fájlt (`FeladatRadar\database\dump\ures.sql`)
3. Futtassa **F5**-tel – ez egyszerre hozza létre az adatbázist, az összes táblát és a tárolt eljárásokat
4. Sikeres futás után az Object Explorerben megjelenik a `FeladatRadar` adatbázis

### 4. appsettings.json beállítása (backend)

A `FeladatRadar.backend/appsettings.json` fájlban cserélje le a `ConnectionStrings > Default` értékét a lokális kapcsolatra:

**SQL Server Authentication:**
```json
"Default": "Server=SAJAT-GEPNEV\\SQLEXPRESS;Database=FeladatRadar;user=sa;password=JELSZO;TrustServerCertificate=true"
```

**Windows Authentication:**
```json
"Default": "Server=.\\SQLEXPRESS;Database=FeladatRadar;Integrated Security=true;TrustServerCertificate=true"
```

> A gépnevet az SSMS kapcsolódási ablakában a **Server name** mezőben láthatja.

### 5. Program.cs beállítása (frontend)

A `FeladatRadar.frontend/Program.cs` fájlban cserélje ki a `BaseAddress` értékét a lokális backend URL-re:

```csharp
BaseAddress = new Uri("https://localhost:44359/")
```

### 6. Multiple Startup Projects beállítása

Hogy egyszerre induljon el a backend és a frontend:

> Jobb klikk a Solution-ön → **Set Startup Projects...** → **Multiple startup projects** → mindkét projektnél (`FeladatRadar.backend` és `FeladatRadar.frontend`) az Action értéke legyen **Start** → OK

Ezt csak egyszer kell beállítani, a Visual Studio elmenti.

### 7. Futtatás

Nyomjon **F5**-öt, vagy kattintson a zöld **Start** gombra.

| Szolgáltatás | Cím |
|---|---|
| Backend (Swagger) | https://localhost:44359/swagger |
| Frontend | https://localhost:7189/ |

> Ha a böngésző SSL-figyelmeztetést jelez, fogadja el a fejlesztői tanúsítványt: **Advanced → Proceed**

---

## Adatbázis előkészítése

A `database\dump\` mappában lévő fájlokat az alábbi sorrendben kell futtatni:

1. `ures.sql` – létrehozza az üres adatbázis sémát és a tárolt eljárásokat
2. `tablafeltoltes.sql` – feltölti a tesztfelhasználókat és a mintaadatokat

**Alternatíva:** A `FeladatRadar.bacpac` fájl visszaállítható SSMS-ben:
> Jobb klikk a **Databases**-en → **Import Data-tier Application...** → válassza ki a `.bacpac` fájlt

---

## Fejlesztői tesztfelhasználók

| Szerepkör | Felhasználónév | Jelszó | Leírás |
|---|---|---|---|
| Student (Diák) | tesztdiak | Teszt123! | Feladatok, órarend, csoportok |
| Teacher (Tanár) | tesztanar | Teszt123! | Dolgozat hozzáadása, tanári csoportkezelés |
| Admin | tesztadmin | Teszt123! | Rendszerkezelés, felhasználókezelés, audit napló |

> A jelszavak BCrypt hash-ként tárolódnak az adatbázis `PasswordHash` mezőjében.
