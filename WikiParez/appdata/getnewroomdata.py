import argparse
import json
import sys
from pathlib import Path

parser = argparse.ArgumentParser(description="Print keys from a JSON file starting at a given key, with incrementing numbers.")
parser.add_argument("key", type=str, help="The key to start from")
parser.add_argument("number", type=int, help="The starting number")
parser.add_argument("date", type=str, help="Date")
args = parser.parse_args()

key_to_start = args.key
start_number = args.number
date = args.date

json_path = Path("pagesData.json")
if not json_path.exists():
    print("Error: pagesData.json not found in the current directory.")
    sys.exit(1)

with open(json_path, "r", encoding="utf-8") as f:
    data = json.load(f)

if not isinstance(data, dict):
    print("Error: JSON root is not a dictionary.")
    sys.exit(1)

found = False
counter = start_number

for key in data:
    if key == key_to_start:
        found = True
    if found:
        name = data[key]["Title"]
        print(f"<li>#{counter} - {date} <a href=\"{key}\">{name}</a></li>")
        counter += 1

if not found:
    print(f"Key '{key_to_start}' not found in pagesData.json.")

