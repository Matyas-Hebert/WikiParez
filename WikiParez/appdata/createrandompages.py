import json
import re
import random

# 1. Load pagesData.json
with open("WikiParez/appdata/pagesData.json", encoding="utf-8") as f:
    pages = json.load(f)

# 2. Filter only room pages
only_room_pages = {k: v for k, v in pages.items() if v.get("Type") == "místnost" and k.startswith("mi")}

only_room_pages = dict(sorted(only_room_pages.items()))

page_keys = list(only_room_pages.keys())
random.shuffle(page_keys)

highways = ["mi_parek", "mi_asfaltka", "mi_dalnice", "mi_dalnicni_pilir", "mi_hraz", "mi_prazce", "mi_slouporadí", "mi_most", "mi_meda", "mi_zavodni_okruh", "mi_mestsky_okruh"]
pseudos = ["mi_sloupek", "mi_jama_pekelna", "mi_samovrazedne_dvere", "mi_ah_fuck", "mi_posvatna_jama"]

# 3. Transform to ParezlePage-like structure
def get_name_from_link(s):
    match = re.match(r"\[([^\]]+)\]\([^)]+\)", s)
    if match:
        return match.group(1)
    return s

parezle_pages = {}
parezle_pages["version"] = input("enter version")
i = 0
for key, page in only_room_pages.items():
    print(page["Title"])
    if (page["coordinates"]["x"] == 0):
        page["patternleCompatible"] = False
    ishighway = False
    if key in highways:
        ishighway = True
    ispseudo = False
    if key in pseudos:
        ispseudo = True

    rp = only_room_pages[page_keys[i]]
    
    parezle_pages[key] = {
        "Title": rp.get("Title"),
        "Bordering_rooms": page.get("Bordering_rooms", []),
        "Blok": get_name_from_link(rp.get("Metadata", {}).get("Blok", "")),
        "Okrsek": get_name_from_link(rp.get("Metadata", {}).get("Okrsek", "")),
        "Ctvrt": get_name_from_link(rp.get("Metadata", {}).get("Čtvrť", "")),
        "Cast": get_name_from_link(rp.get("Metadata", {}).get("Část", "")),
        "PatternleCompatible": page["patternleCompatible"],
        "IsHighway": ishighway,
        "IsPseudo": ispseudo,
        "Coordinates": {
            "x": page["coordinates"]["x"],
            "y": page["coordinates"]["y"],
            "z": page["coordinates"]["z"]
        }
    }
    i+=1

# 4. Write to parezlepages.json
with open("WikiParez/appdata/randomparezlepages.json", "w", encoding="utf-8") as f:
    json.dump(parezle_pages, f, ensure_ascii=False, indent=2)

print("Saved parezlepages.json successfully!")
