"""
MyVibik Assistant — Помощник для проекта "Мой Вайбик"
Автоматически корректирует путь к папке Assets/Scripts.
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
                self.fix_scripts_path()  # <-- добавляем коррекцию пути
            except Exception as e:
                print(f"[ERROR] Ошибка загрузки: {e}")
                self.create_default_config()
        else:
            self.create_default_config()

    def fix_missing_keys(self):
        default = self.get_default_config()
        self.merge_dicts(self.data, default)
        self.save_config()

    def fix_scripts_path(self):
        """Исправляет путь к папке Assets/Scripts, если он указывает не туда"""
        current_dir = os.path.dirname(os.path.abspath(__file__))
        scripts_rel = self.data['folders'].get('scripts', 'Assets/Scripts')
        # Проверяем, существует ли папка по этому пути
        test_path = os.path.abspath(os.path.join(current_dir, scripts_rel))
        if not os.path.exists(test_path):
            # Пробуем вариант просто "Assets/Scripts" (относительно папки скрипта)
            new_path = "Assets/Scripts"
            new_test_path = os.path.abspath(os.path.join(current_dir, new_path))
            if os.path.exists(new_test_path) or not os.path.exists(new_test_path):
                # Если папка не существует, но путь корректен – оставляем
                if scripts_rel != new_path:
                    print(f"[INFO] Исправляю путь к скриптам: {scripts_rel} → {new_path}")
                    self.data['folders']['scripts'] = new_path
                    self.save_config()
            else:
                # Если папка не существует по новому пути, тоже оставляем (её создадут позже)
                if scripts_rel != new_path:
                    print(f"[INFO] Исправляю путь к скриптам: {scripts_rel} → {new_path}")
                    self.data['folders']['scripts'] = new_path
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
        # Создаём папку, если её нет (внутри папки со скриптом)
        self.ensure_scripts_folder()

    def get_default_config(self):
        return {
            "project": {
                "name": "Мой Вайбик",
                "unity_version": "2022.3 LTS",
                "type": "2D Tamagotchi-симулятор заботы",
                "description": "Игра для детей 6-9 лет: гладь, корми и играй с милым существом Вайбиком. Собирай наклейки, открывай костюмы и фоны. Простая, добрая и без стресса.",
                "created": str(datetime.now())
            },
            "rules": {
                "naming": {
                    "переменные": "camelCase (playerHealth, currentTurn)",
                    "методы": "PascalCase (CalculateDamage(), StartBattle())",
                    "классы": "PascalCase (GameManager, VibikCard)",
                    "файлы": "PascalCase (GameManagerAutotest.cs)",
                    "character": "Character (не Unit)",
                    "damage": "damageValue",
                    "health": "currentHP",
                    "mana": "energy"
                },
                "architecture": [
                    "Использовать ScriptableObjects для данных",
                    "Избегать синглтонов",
                    "Использовать события (UnityEvent)",
                    "Все UI-элементы создавать в Unity, по возможности без скриптов"
                ]
            },
            "folders": {
                "scripts": "Assets/Scripts",
                "prefabs": "Assets/Prefabs",
                "scenes": "Assets/Scenes",
                "sprites": "Assets/Sprites",
                "audio": "Assets/Audio"
            },
            "history": {
                "scans": [],
                "requests": []
            },
            "script_structure": []
        }

    def ensure_scripts_folder(self):
        """Создаёт папку Assets/Scripts, если её нет"""
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

        # Проверяем, существует ли папка, если нет – создаём
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
‼️ ВНИМАНИЕ: ЧИТАЙ ВСЕ ИНСТРУКЦИИ ПРЕЖДЕ ЧЕМ ОТВЕЧАТЬ

1. 📁 ПЕРВЫЙ И САМЫЙ ВАЖНЫЙ ШАГ - ЗАПРОС ФАЙЛОВ
   • Если для выполнения задачи нужны какие-то файлы (скрипты, сцены, префабы и т.д.) - ЗАПРОСИ ИХ ПРЯМО СЕЙЧАС
   • Не пытайся угадывать код или структуру файлов без их содержимого
   • Скажи мне КОНКРЕТНО какие файлы тебе нужны (например: "Пришли мне GameManager.cs и UIManager.cs")

2. 🚫 ЧТО НЕ ДЕЛАТЬ:
   • НЕ ПЫТАЙСЯ писать код без просмотра реальных файлов
   • НЕ ДЕЛАЙ предположений о структуре классов без их исходного кода
   • НЕ ПРЕДЛАГАЙ изменения, не видя текущую реализацию

3. ✅ ЧТО ДЕЛАТЬ ПОСЛЕ ПОЛУЧЕНИЯ ФАЙЛОВ:
   • Только после получения ВСЕХ необходимых файлов анализируй их содержимое
   • Учитывай контекст проекта выше (правила именования, архитектурные правила)
   • Предлагай изменения на основе реального кода

4. 📝 ФОРМАТ ОТВЕТА (только после получения всех файлов):
   1. 📊 Анализ текущей ситуации (на основе полученных файлов)
   2. 🔧 Предлагаемые изменения (с учетом контекста проекта)
   3. 📄 Полный исправленный код (если меняешь существующие файлы)
   4. ⚠️ Важные замечания

5. Я начинающий соло-разработчик. Учти это составляя инструкции или формулируя ответ.

6. Весь UI создаем в Unity. Пиши подробные инструкции как это делать. По возможности не используем скрипты для UI-логики (используй UnityEvents в инспекторе).

7. никогда сам ничего не добавляй. выполняй только четко поставленную задачу. если есть какие то предложения по улучшению предложи в чате. ни в коем случае не в коде.

8. 🔴 КРИТИЧЕСКОЕ ДЛЯ WEBGL (Яндекс.Игры):
   • Мы используем локализацию (русский, в будущем английский). Всегда добавляй ожидание готовности локализации (корутины, проверки isLocalizationReady) перед вызовом методов, требующих локализации.
   • НИКОГДА не используй синхронные методы загрузки Addressables (WaitForCompletion) — они не поддерживаются на WebGL и вызывают фатальную ошибку.
   • Все вызовы, связанные с инициализацией (RestartGame, SetAILevel), должны быть защищены проверками на null (например, enhancedLogger) и откладываться, если объект ещё не готов.
   • Помни: в WebGL объекты инициализируются медленнее, порядок вызовов может отличаться от редактора.

9. ВСЕГДА пиши скрипты полностью (весь файл), не используй сокращения.
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

        print("\nОпишите, что вы хотите сделать (например, 'Добавить магазин костюмов'):")
        query = input("Ваш запрос: ").strip()
        if not query:
            print("[ERROR] Запрос не может быть пустым")
            return

        prompt = self.build_prompt(query)

        print("\n" + "="*50)
        print("ГОТОВЫЙ ПРОМПТ ДЛЯ DEEPSEEK")
        print("="*50)
        print("\n" + prompt)
        print("\n" + "="*50)

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
            print("1. 🔍 Сканировать структуру скриптов (Assets/Scripts)")
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