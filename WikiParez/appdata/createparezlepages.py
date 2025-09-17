import json
import re

# 1. Load pagesData.json
with open("WikiParez/appdata/pagesData.json", encoding="utf-8") as f:
    pages = json.load(f)

# 2. Filter only room pages
only_room_pages = {k: v for k, v in pages.items() if v.get("Type") == "místnost" and k.startswith("mi")}

# 3. Transform to ParezlePage-like structure
def get_name_from_link(s):
    match = re.match(r"\[([^\]]+)\]\([^)]+\)", s)
    if match:
        return match.group(1)
    return s

parezle_pages = {}
parezle_pages["version"] = input("enter version")
for key, page in only_room_pages.items():
    if (page["coordinates"]["x"] == 0):
        page["patternleCompatible"] = False
    parezle_pages[key] = {
        "Title": page.get("Title"),
        "Bordering_rooms": page.get("Bordering_rooms", []),
        "Blok": get_name_from_link(page.get("Metadata", {}).get("Blok", "")),
        "Okrsek": get_name_from_link(page.get("Metadata", {}).get("Okrsek", "")),
        "Ctvrt": get_name_from_link(page.get("Metadata", {}).get("Čtvrť", "")),
        "Cast": get_name_from_link(page.get("Metadata", {}).get("Část", "")),
        "PatternleCompatible": page["patternleCompatible"],
        "Coordinates": {
            "x": page["coordinates"]["x"],
            "y": page["coordinates"]["y"],
            "z": page["coordinates"]["z"]
        }
    }

# 4. Write to parezlepages.json
with open("WikiParez/appdata/parezlepages.json", "w", encoding="utf-8") as f:
    json.dump(parezle_pages, f, ensure_ascii=False, indent=2)

print("Saved parezlepages.json successfully!")
