"""
MyVibik Assistant — Помощник для проекта "Мой Вайбик"
Поддержка VK Mini App и RuStore
"""

import json
import os
from datetime import datetime

class MyVibikConfig:
    def __init__(self):
        self.config_file = "myvibik_project.json"
        self.load_config()

    def load_config(self):
        if os.path.exists(self.config_file):
            try:
                with open(self.config_file, 'r', encoding='utf-8') as f:
                    self.data = json.load(f)
                print("[OK] Конфигурация загружена")
                self.fix_missing_keys()
            except Exception as e:
                print(f"[ERROR] Ошибка загрузки: {e}")
                self.create_default_config()
        else:
            self.create_default_config()

    def fix_missing_keys(self):
        default = self.get_default_config()
        self.merge_dicts(self.data, default)
        self.save_config()

    def merge_dicts(self, target, source):
        for key, value in source.items():
            if key not in target:
                target[key] = value
            elif isinstance(value, dict) and isinstance(target.get(key), dict):
                self.merge_dicts(target[key], value)

    def create_default_config(self):
        print("[NEW] Создаю новый конфиг...")
        self.data = self.get_default_config()
        self.data['folders']['scripts'] = "Assets/Scripts"
        self.save_config()
        self.ensure_scripts_folder()

    def get_default_config(self):
        return {
            "project": {
                "name": "Мой Вайбик",
                "unity_version": "2022.3 LTS",
                "type": "2D Tamagotchi-симулятор (VK Mini App + RuStore)",
                "description": "Игра-витрина для мягкой игрушки с механикой объятия. VK Mini App для раскрутки, RuStore для монетизации.",
                "created": str(datetime.now())
            },
            "rules": {
                "naming": {
                    "переменные": "camelCase",
                    "методы": "PascalCase",
                    "классы": "PascalCase",
                    "файлы": "PascalCase",
                    "интерфейсы": "I" + "PascalCase"
                },
                "architecture": [
                    "Использовать ScriptableObjects для данных",
                    "Платформенный слой через IPlatformService",
                    "Избегать синглтонов (кроме PlatformService)",
                    "Все UI-элементы создавать в Unity"
                ]
            },
            "folders": {
                "scripts": "Assets/Scripts",
                "prefabs": "Assets/Prefabs",
                "scenes": "Assets/Scenes",
                "sprites": "Assets/Sprites",
                "audio": "Assets/Audio",
                "resources": "Assets/Resources"
            },
            "history": {
                "scans": [],
                "requests": []
            },
            "script_structure": [],
            "platforms": ["VK", "RuStore", "GooglePlay"]
        }

    def ensure_scripts_folder(self):
        scripts_path = self.data['folders'].get('scripts', 'Assets/Scripts')
        current_dir = os.path.dirname(os.path.abspath(__file__))
        abs_scripts_path = os.path.abspath(os.path.join(current_dir, scripts_path))
        if not os.path.exists(abs_scripts_path):
            os.makedirs(abs_scripts_path, exist_ok=True)
            print(f"[INFO] Создана папка: {abs_scripts_path}")
        return abs_scripts_path

    def save_config(self):
        with open(self.config_file, 'w', encoding='utf-8') as f:
            json.dump(self.data, f, ensure_ascii=False, indent=2)
        print("[SAVE] Конфигурация сохранена")

    def scan_scripts(self):
        print("\n" + "="*50)
        print("СКАНИРОВАНИЕ СТРУКТУРЫ СКРИПТОВ")
        print("="*50)

        scripts_path = self.data['folders'].get('scripts', 'Assets/Scripts')
        current_dir = os.path.dirname(os.path.abspath(__file__))
        abs_scripts_path = os.path.abspath(os.path.join(current_dir, scripts_path))

        if not os.path.exists(abs_scripts_path):
            print(f"[WARN] Папка не найдена: {abs_scripts_path}")
            print("[INFO] Создаю папку...")
            os.makedirs(abs_scripts_path, exist_ok=True)
            print(f"[OK] Папка создана: {abs_scripts_path}")
            self.data['script_structure'] = []
            self.data['history']['scans'].append({
                "date": str(datetime.now()),
                "count": 0
            })
            self.save_config()
            print("\nСтруктура скриптов: (папка пуста)")
            input("\nНажмите Enter для продолжения...")
            return

        script_files = []
        for root, _, files in os.walk(abs_scripts_path):
            for file in files:
                if file.endswith('.cs'):
                    rel_path = os.path.relpath(os.path.join(root, file), abs_scripts_path)
                    script_files.append(rel_path)

        self.data['script_structure'] = sorted(script_files)
        self.data['history']['scans'].append({
            "date": str(datetime.now()),
            "count": len(script_files)
        })
        self.save_config()

        print(f"[OK] Найдено {len(script_files)} скриптов")
        if script_files:
            print("\nСтруктура скриптов:")
            folders = {}
            for path in script_files:
                if '/' in path:
                    folder = os.path.dirname(path)
                    file = os.path.basename(path)
                    folders.setdefault(folder, []).append(file)
                else:
                    folders.setdefault("Root", []).append(path)
            for folder in sorted(folders.keys()):
                if folder == "Root":
                    for f in sorted(folders[folder]):
                        print(f"  • {f}")
                else:
                    print(f"  📁 {folder}/")
                    for f in sorted(folders[folder]):
                        print(f"    • {f}")
        else:
            print("  (нет скриптов)")

        input("\nНажмите Enter для продолжения...")

    def build_prompt(self, query):
        scripts_text = ""
        if self.data['script_structure']:
            scripts_text = "📁 СТРУКТУРА СКРИПТОВ (Assets/Scripts):\n"
            folders = {}
            for path in self.data['script_structure']:
                if '/' in path:
                    folder = os.path.dirname(path)
                    file = os.path.basename(path)
                    folders.setdefault(folder, []).append(file)
                else:
                    folders.setdefault("Root", []).append(path)
            for folder in sorted(folders.keys()):
                if folder == "Root":
                    for f in sorted(folders[folder]):
                        scripts_text += f"  • {f}\n"
                else:
                    scripts_text += f"  📁 {folder}/\n"
                    for f in sorted(folders[folder]):
                        scripts_text += f"    • {f}\n"
        else:
            scripts_text = "📁 СТРУКТУРА СКРИПТОВ: не сканирована (используйте пункт 1)\n"

        prompt = f"""
КОНТЕКСТ ПРОЕКТА UNITY:
=======================
Проект: {self.data['project']['name']}
Тип: {self.data['project']['type']}
Версия Unity: {self.data['project']['unity_version']}
Краткое описание: {self.data['project']['description']}

📏 ПРАВИЛА ИМЕНОВАНИЯ:
{json.dumps(self.data['rules']['naming'], indent=2, ensure_ascii=False)}

🏗️ АРХИТЕКТУРНЫЕ ПРАВИЛА:
{chr(10).join(f"  • {rule}" for rule in self.data['rules']['architecture'])}

{scripts_text}

🎯 ЗАДАЧА ПОЛЬЗОВАТЕЛЯ:
=======================
{query}

🔄 ВАЖНЫЕ ИНСТРУКЦИИ ДЛЯ DEEPSEEK:
1. Первый шаг — запроси файлы, если они нужны.
2. Не пиши код без просмотра реальных файлов.
3. Учитывай платформенную абстракцию (IPlatformService).
4. Пиши скрипты полностью.

🔴 КРИТИЧЕСКИЕ ТРЕБОВАНИЯ:
- Платформенный слой через IPlatformService
- Поддержка VK Mini App (WebGL) и RuStore (Android)
- Облачное сохранение через VK ID
- Тизер контента в VK-версии («Доступно в приложении»)
"""
        return prompt

    def copy_to_clipboard(self, text):
        try:
            import pyperclip
            pyperclip.copy(text)
            print("\n[OK] Промпт скопирован в буфер обмена!")
        except ImportError:
            print("\n[INFO] pyperclip не установлен. Скопируйте текст вручную.")

    def create_request(self):
        print("\n" + "="*50)
        print("СОЗДАНИЕ ЗАПРОСА ДЛЯ DEEPSEEK")
        print("="*50)

        print("\nОпишите, что вы хотите сделать:")
        print("Пример: 'Создать IPlatformService и заглушку'")
        query = input("Ваш запрос: ").strip()
        if not query:
            print("[ERROR] Запрос не может быть пустым")
            return

        prompt = self.build_prompt(query)

        print("\n" + "="*50)
        print("ГОТОВЫЙ ПРОМПТ")
        print("="*50)
        print("\n" + prompt)

        self.copy_to_clipboard(prompt)

        self.data['history']['requests'].append({
            "date": str(datetime.now()),
            "query": query
        })
        self.save_config()

        input("\nНажмите Enter для продолжения...")

    def show_menu(self):
        while True:
            print("\n" + "="*50)
            print(f"MYVIBIK ASSISTANT — {self.data['project']['name']}")
            print("="*50)
            print("1. 🔍 Сканировать структуру скриптов")
            print("2. 📝 Создать запрос для DeepSeek")
            print("3. 🚪 Выход")

            choice = input("\nВаш выбор (1-3): ").strip()

            if choice == "1":
                self.scan_scripts()
            elif choice == "2":
                self.create_request()
            elif choice == "3":
                print("До встречи!")
                break
            else:
                print("[ERROR] Неверный выбор")

def main():
    print("\n" + "="*50)
    print("ЗАПУСК MYVIBIK ASSISTANT")
    print("="*50)
    config = MyVibikConfig()
    config.show_menu()

if __name__ == "__main__":
    main()