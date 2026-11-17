"""Generate and validate the CryptEscape dungeon layout (ASCII)."""
import sys
from collections import deque

W, H = 37, 29
g = [["#"] * W for _ in range(H)]


def room(r0, r1, c0, c1, ch="."):
    for r in range(r0, r1 + 1):
        for c in range(c0, c1 + 1):
            g[r][c] = ch


def put(r, c, ch):
    assert g[r][c] in ".,", f"({r},{c}) is {g[r][c]!r}, cannot place {ch}"
    g[r][c] = ch


# exit chamber and boss arena
room(1, 1, 16, 20)
g[2][18] = "E"
room(3, 10, 11, 25, ",")
g[11][18] = "D"
# side rooms north
room(3, 8, 1, 9)    # NW
room(3, 8, 27, 35)  # NE
# antechamber
room(12, 13, 12, 24)
# west / east wings
room(14, 19, 1, 9)    # W1
room(22, 27, 1, 9)    # W2
room(14, 19, 27, 35)  # E1
room(22, 27, 27, 35)  # E2
# central hall and start room
room(16, 21, 12, 24)
room(23, 27, 14, 22)
# corridors
for r in range(9, 14):
    g[r][5] = "."
    g[r][31] = "."
for r in (14, 15):
    g[r][18] = "."
g[22][18] = "."
for c in (10, 11):
    g[17][c] = "."
for c in (25, 26):
    g[17][c] = "."
for c in range(10, 14):
    g[25][c] = "."
for c in range(23, 27):
    g[25][c] = "."
for r in (20, 21):
    g[r][5] = "."
    g[r][31] = "."

# contents
put(1, 18, "X")
put(6, 18, "B")
put(26, 18, "@")
put(26, 15, "A")  # altar 1: start room
put(17, 14, "A")  # altar 2: central hall
put(12, 15, "A")  # altar 3: antechamber
# coins
put(16, 2, "C")   # W1
put(26, 2, "C")   # W2
put(16, 34, "C")  # E1
put(26, 34, "C")  # E2
put(4, 3, "C")    # NW
# potions
put(23, 8, "P")   # W2
put(5, 32, "P")   # NE
put(12, 22, "P")  # antechamber
# slimes
for r, c in [(15, 6), (18, 7), (7, 7), (24, 30), (26, 31), (19, 21)]:
    put(r, c, "S")
# goblins
for r, c in [(24, 5), (5, 6), (17, 30), (6, 29), (18, 16)]:
    put(r, c, "G")

rows = ["".join(r) for r in g]

# validate: reachability from start through walkable cells (D and E count as walkable)
walk = set(".,@ACPSGBXDE")
start = next((r, c) for r in range(H) for c in range(W) if g[r][c] == "@")
seen = {start}
q = deque([start])
while q:
    r, c = q.popleft()
    for dr, dc in ((1, 0), (-1, 0), (0, 1), (0, -1)):
        nr, nc = r + dr, c + dc
        if 0 <= nr < H and 0 <= nc < W and (nr, nc) not in seen and g[nr][nc] in walk:
            seen.add((nr, nc))
            q.append((nr, nc))

targets = [(r, c, g[r][c]) for r in range(H) for c in range(W) if g[r][c] in "CABXPSGDE"]
unreached = [t for t in targets if (t[0], t[1]) not in seen]
counts = {ch: sum(row.count(ch) for row in rows) for ch in "CASGPBXDE@"}
print("counts", counts, file=sys.stderr)
print("unreached", unreached, file=sys.stderr)
assert not unreached
assert counts["C"] == 5 and counts["A"] == 3 and counts["B"] == 1

legend = [
    "# CryptEscape dungeon layout. 1 cell = 2m. Top of the map = +Z.",
    "# Symbols: # wall, . floor, , boss arena floor, @ start, C coin, A altar (save point),",
    "#          P potion, S slime, G goblin, B boss, D boss door, E exit gate, X exit zone",
    "# Lines starting with '#' followed by a space are comments.",
]
sys.stdout.write("\n".join(legend + rows) + "\n")
