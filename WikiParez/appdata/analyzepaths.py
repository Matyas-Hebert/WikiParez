import json
import re
from collections import deque, defaultdict
from concurrent.futures import ThreadPoolExecutor, as_completed

json_data = open('WikiParez/appdata/pagesData.json', 'r', encoding='utf-8').read()
data = json.loads(json_data)

def find_paths(room_from, room_to):
    to_explore = deque()
    distance = {}
    predecessors = defaultdict(list)

    distance[room_from] = 0
    to_explore.append(room_from)

    while to_explore:
        current_room = to_explore.popleft()
        current_distance = distance[current_room]
        bordering_rooms = get_bordering(current_room)

        for neighbor in bordering_rooms:
            if neighbor not in distance:
                distance[neighbor] = current_distance + 1
                predecessors[neighbor].append(current_room)
                to_explore.append(neighbor)
            elif distance[neighbor] == current_distance + 1:
                predecessors[neighbor].append(current_room)

    if room_to not in predecessors:
        return []

    all_paths = []

    def backtrack(current, path):
        if current == room_from:
            complete_path = path + [room_from]
            complete_path.reverse()
            all_paths.append(complete_path)
            return
        for prev in predecessors[current]:
            backtrack(prev, path + [current])

    backtrack(room_to, [])

    return all_paths

def get_bordering(room):
    return data[room]["Bordering_rooms"]

def does_border(room1, room2):
    if (room2 in data[room1]["Bordering_rooms"]):
        return True
    return False

only_rooms_pages = {}
for key in data.keys():
    if (data[key]["Type"] == "místnost" and "mi_" in key):
        only_rooms_pages[key] = data[key]

scores = {}
i = 0
total = len(only_rooms_pages)

for room1 in only_rooms_pages.keys():
    print("Analyzing Paths:", round(str(i+1) + "/" + str(total)))
    i+=1
    for room2 in only_rooms_pages.keys():
        if room1 != room2 and not does_border(room1, room2):
            paths = find_paths(room1, room2)
            number_of_paths = len(paths)
            for path in paths:
                for room in path:
                    if room != room1 and room != room2:
                        scores[room] = scores.get(room, 0) + 1.0 / number_of_paths

# Sort by descending score
sorted_scores = sorted(scores.items(), key=lambda kv: kv[1], reverse=True)

print(sorted_scores)
with open("WikiParez/appdata/parezlescores.json", "w") as f:
    json.dump(sorted_scores, f)
