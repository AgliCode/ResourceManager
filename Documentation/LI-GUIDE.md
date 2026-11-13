# ResourceManager CLI – Quick Start Guide

## 1. How to open the terminal

After installing ResourceManager:

1. Open the folder where ResourceManager was installed. By default, it is:

   `C:\Program Files\Resource Manager`

2. Open the `CLI` folder.


## 2. Available commands

To see all available commands, run:

```powershell
.\ResourceManager.CLI.exe help
```

### First-time setup

Before using the CLI, configure your project:

```powershell
.\ResourceManager.CLI.exe setup
```

During setup, you will be asked to:

- Configure your Gemini API key (optional).
- Select your project directory.
- Choose the Base RESX file.
- Choose the Target RESX file.
- Select the target language.

Your configuration will be saved locally, so you do not need to repeat this process every time.

### Display project status

```powershell
.\ResourceManager.CLI.exe status
```

Displays information about your current configuration, including:

- Project directory.
- Base and Target RESX files.
- Total number of resource keys.
- Number of missing translations.
- Number of translated keys.
- Gemini API key configuration status.

### Find missing translations

```powershell
.\ResourceManager.CLI.exe missing
```

Displays all entries that are missing translations in the Target RESX file.

### Translate missing entries using AI

```powershell
.\ResourceManager.CLI.exe missing --translate
```

Uses Gemini AI to translate missing entries.

Translations are generated in batches. After generation, you can choose how to approve them:

1. **Approve all translations** – saves all generated translations to the Target RESX file.
2. **Review translations individually** – lets you approve or skip each translation separately.
3. **Cancel** – discards the generated suggestions without saving them.

Only approved translations are saved to the Target RESX file.

### Change project configuration

```powershell
.\ResourceManager.CLI.exe configure
```

Allows you to modify individual settings:

1. Project directory.
2. Base RESX file.
3. Target RESX file.
4. Target language.
5. Gemini API key.
6. Back.

## 3. Important information

- The `setup` command only needs to be run when configuring a project for the first time or setting up a new project.
- Project configuration is saved locally on your computer.
- A Gemini API key is required for AI translation features.
- Without a Gemini API key, you can still browse missing translations using the `missing` command.

## 4. Example workflow

A typical workflow looks like this:

```powershell
# Configure your project (first use)
.\ResourceManager.CLI.exe setup

# Check project status
.\ResourceManager.CLI.exe status

# Find missing translations
.\ResourceManager.CLI.exe missing

# Generate AI translations
.\ResourceManager.CLI.exe missing --translate
```
