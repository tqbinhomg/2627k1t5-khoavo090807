import tkinter as tk
from neo4j import GraphDatabase
import networkx as nx

# --- CẤU HÌNH KẾT NỐI NEO4J ---
URI = "neo4j://127.0.0.1:7687"
AUTH = ("neo4j", "12345678") # Sửa lại mật khẩu của bạn

class FullFeatureGraphApp:
    def __init__(self, root):
        self.root = root
        self.root.title("Graph Tri thức Hình học - Đầy đủ tính năng")
        self.root.geometry("1200x850")

        self.nodes_data = {}  
        self.edges_data = []  
        self.drag_data = {"node": None, "start_x": 0, "start_y": 0}
        self.node_radius = 42 

        self.setup_ui()
        self.load_graph_from_neo4j()

    def setup_ui(self):
        # 1. Canvas vẽ đồ thị Graph (Nửa trên)
        self.canvas = tk.Canvas(self.root, bg="#f0f0f0", height=420)
        self.canvas.pack(fill=tk.BOTH, expand=True)

        # 2. Bảng thông tin phía dưới (Chia làm 3 cột: Lý thuyết, Chia sẻ tính chất, Vẽ hình)
        self.frame_bottom = tk.Frame(self.root, height=380, bg="white", padx=10, pady=10)
        self.frame_bottom.pack(fill=tk.X, side=tk.BOTTOM)
        self.frame_bottom.pack_propagate(False)

        self.lbl_selected_title = tk.Label(self.frame_bottom, text="HÃY CLICK VÀO MỘT NODE TRÊN BIỂU ĐỒ", font=("Arial", 11, "bold"), fg="#c0392b", bg="white")
        self.lbl_selected_title.pack(anchor="w", pady=(0, 5))

        # --- CỘT 1: LÝ THUYẾT & CÔNG THỨC (Trái) ---
        self.frame_col1 = tk.Frame(self.frame_bottom, bg="white", width=360, bd=1, relief="solid", padx=8, pady=5)
        self.frame_col1.pack(side=tk.LEFT, fill=tk.BOTH, padx=5)
        self.frame_col1.pack_propagate(False)

        tk.Label(self.frame_col1, text="📖 Thông tin từ Neo4j:", font=("Arial", 10, "bold"), bg="white", fg="#2980b9").pack(anchor="w")
        self.lbl_info_desc = tk.Label(self.frame_col1, text="", font=("Arial", 9), justify="left", wraplength=340, bg="white")
        self.lbl_info_desc.pack(anchor="w", pady=(2, 5))

        self.lbl_math_title = tk.Label(self.frame_col1, text="📐 Công thức Toán học:", font=("Arial", 10, "bold"), bg="white", fg="#27ae60")
        self.lbl_math_title.pack(anchor="w")
        self.lbl_info_math = tk.Label(self.frame_col1, text="", font=("Arial", 9), justify="left", bg="white")
        self.lbl_info_math.pack(anchor="w", pady=(2, 5))

        # --- CỘT 2: TÍNH CHẤT CHIA SẺ (Giữa) ---
        self.frame_col2 = tk.Frame(self.frame_bottom, bg="#f9f9f9", width=380, bd=1, relief="solid", padx=8, pady=5)
        self.frame_col2.pack(side=tk.LEFT, fill=tk.BOTH, padx=5)
        self.frame_col2.pack_propagate(False)

        tk.Label(self.frame_col2, text="🔗 Tính chất chia sẻ chung:", font=("Arial", 10, "bold"), bg="#f9f9f9", fg="#8e44ad").pack(anchor="w", pady=(0, 5))
        self.listbox_shared = tk.Listbox(self.frame_col2, font=("Arial", 8), height=14)
        self.listbox_shared.pack(fill=tk.BOTH, expand=True)

        # --- CỘT 3: VẼ HÌNH MINH HỌA (Phải) ---
        self.frame_col3 = tk.Frame(self.frame_bottom, bg="#ecf0f1", width=380, bd=1, relief="solid", padx=8, pady=5)
        self.frame_col3.pack(side=tk.RIGHT, fill=tk.BOTH, padx=5)
        self.frame_col3.pack_propagate(False)

        tk.Label(self.frame_col3, text="🎨 Mô phỏng hình học:", font=("Arial", 10, "bold"), bg="#ecf0f1", fg="#2c3e50").pack(anchor="w", pady=(0, 2))
        self.shape_canvas = tk.Canvas(self.frame_col3, bg="white", width=350, height=260)
        self.shape_canvas.pack(pady=2)

        # Gắn sự kiện chuột kéo thả
        self.canvas.bind("<ButtonPress-1>", self.on_press)
        self.canvas.bind("<B1-Motion>", self.on_drag)
        self.canvas.bind("<ButtonRelease-1>", self.on_release)

    def load_graph_from_neo4j(self):
        self.driver = GraphDatabase.driver(URI, auth=AUTH)
        try:
            with self.driver.session() as session:
                query = """
                MATCH (s:Shape)-[r:HAS_PROPERTY]->(p:Property)
                RETURN s.name AS source, p.name AS target
                """
                results = session.run(query).data()
        except Exception as e:
            self.lbl_selected_title.config(text=f"Lỗi kết nối CSDL: {e}")
            return

        G = nx.Graph()
        edges = []
        for row in results:
            src, tgt = row["source"], row["target"]
            edges.append((src, tgt))
            G.add_edge(src, tgt)

        pos = nx.spring_layout(G, seed=42, k=0.4)

        self.root.update()
        canvas_width = self.canvas.winfo_width()
        canvas_height = 400

        for src, tgt in edges:
            line_id = self.canvas.create_line(0, 0, 0, 0, width=1.5, fill="#bdc3c7")
            self.edges_data.append({'line': line_id, 'src': src, 'tgt': tgt})

        for node in G.nodes():
            x, y = pos[node]
            px = (x + 1) * (canvas_width / 2 * 0.8) + 60
            py = (y + 1) * (canvas_height / 2 * 0.8) + 30

            is_property = "Có" in node or "Tất cả" in node
            node_color = "#e67e22" if is_property else "#3498db"
            radius = 45 if is_property else self.node_radius

            oval_id = self.canvas.create_oval(px - radius, py - radius, px + radius, py + radius, 
                                              fill=node_color, outline="black", width=2)
            
            display_name = node.replace(" ", "\n")
            text_id = self.canvas.create_text(px, py, text=display_name, font=("Arial", 8, "bold"), fill="white", justify="center")

            self.nodes_data[node] = {'oval': oval_id, 'text': text_id, 'x': px, 'y': py, 'radius': radius}

        self.update_edges_positions()

    def update_edges_positions(self):
        for edge in self.edges_data:
            if edge['src'] in self.nodes_data and edge['tgt'] in self.nodes_data:
                s = self.nodes_data[edge['src']]
                t = self.nodes_data[edge['tgt']]
                self.canvas.coords(edge['line'], s['x'], s['y'], t['x'], t['y'])

    def on_press(self, event):
        clicked_node = None
        for node_name, data in self.nodes_data.items():
            dist = ((event.x - data['x'])**2 + (event.y - data['y'])**2)**0.5
            if dist <= data['radius']:
                clicked_node = node_name
                break

        if clicked_node:
            self.drag_data["node"] = clicked_node
            self.drag_data["start_x"] = event.x
            self.drag_data["start_y"] = event.y
            
            for n_name, n_data in self.nodes_data.items():
                is_prop = "Có" in n_name or "Tất cả" in n_name
                default_color = "#e67e22" if is_prop else "#3498db"
                self.canvas.itemconfig(n_data['oval'], fill=default_color)
            self.canvas.itemconfig(self.nodes_data[clicked_node]['oval'], fill="#e74c3c")
            
            self.fetch_node_details(clicked_node)
            self.draw_shape_illustration(clicked_node)

    def on_drag(self, event):
        if self.drag_data["node"]:
            node_name = self.drag_data["node"]
            dx = event.x - self.drag_data["start_x"]
            dy = event.y - self.drag_data["start_y"]

            self.canvas.move(self.nodes_data[node_name]['oval'], dx, dy)
            self.canvas.move(self.nodes_data[node_name]['text'], dx, dy)

            self.nodes_data[node_name]['x'] += dx
            self.nodes_data[node_name]['y'] += dy

            self.drag_data["start_x"] = event.x
            self.drag_data["start_y"] = event.y

            self.update_edges_positions()

    def on_release(self, event):
        self.drag_data["node"] = None

    def fetch_node_details(self, node_name):
        self.lbl_selected_title.config(text=f"ĐANG CHỌN: {node_name.upper()}")
        self.listbox_shared.delete(0, tk.END)

        try:
            with self.driver.session() as session:
                is_prop_node = "Có" in node_name or "Tất cả" in node_name

                if not is_prop_node:
                    # Lấy lý thuyết từ Neo4j
                    query_shape = "MATCH (s:Shape {name: $name}) RETURN s.tinh_chat AS tc, s.chu_vi AS cv, s.dien_tich AS dt"
                    rec = session.run(query_shape, name=node_name).single()
                    
                    if rec:
                        self.lbl_info_desc.config(text=rec["tc"] if rec["tc"] else "Đang cập nhật")
                        self.lbl_math_title.pack(anchor="w")
                        self.lbl_info_math.pack(anchor="w")
                        self.lbl_info_math.config(text=f"• Chu vi: {rec['cv']}\n• Diện tích: {rec['dt']}")

                    # Lấy tính chất chia sẻ
                    query_shared = """
                    MATCH (s:Shape {name: $name})-[:HAS_PROPERTY]->(p:Property)
                    OPTIONAL MATCH (other:Shape)-[:HAS_PROPERTY]->(p)
                    WHERE other.name <> $name
                    RETURN p.name AS prop, COLLECT(other.name) AS shared_shapes
                    """
                    results = session.run(query_shared, name=node_name).data()
                    
                    for r in results:
                        prop_name = r["prop"]
                        shared = r["shared_shapes"]
                        if shared:
                            self.listbox_shared.insert(tk.END, f"• [{prop_name}]")
                            self.listbox_shared.insert(tk.END, f"   Chung với: {', '.join(shared)}")
                        else:
                            self.listbox_shared.insert(tk.END, f"• [{prop_name}] (Đặc trưng)")
                else:
                    self.lbl_info_desc.config(text=f"Thuộc tính/Tính chất hình học dùng chung trong Knowledge Graph.")
                    self.lbl_math_title.pack_forget()
                    self.lbl_info_math.pack_forget()

                    query_prop_shapes = "MATCH (p:Property {name: $name})<-[:HAS_PROPERTY]-(s:Shape) RETURN s.name AS shape_name"
                    shapes = session.run(query_prop_shapes, name=node_name).data()
                    self.listbox_shared.insert(tk.END, f"✨ Các hình sở hữu tính chất này:")
                    for s in shapes:
                        self.listbox_shared.insert(tk.END, f"   - {s['shape_name']}")

        except Exception as e:
            self.lbl_info_desc.config(text=f"Lỗi: {e}")

    def draw_shape_illustration(self, shape_name):
        """Vẽ hình minh họa trực quan khi click vào node hình học"""
        self.shape_canvas.delete("all")
        cx, cy = 175, 130
        
        is_prop_node = "Có" in shape_name or "Tất cả" in shape_name
        if is_prop_node:
            self.shape_canvas.create_text(cx, cy, text="Node Tính chất\n(Xem danh sách ở cột giữa)", justify="center", font=("Arial", 9, "italic"), fill="gray")
            return

        if shape_name == "Hình vuông":
            self.shape_canvas.create_rectangle(cx-55, cy-55, cx+55, cy+55, outline="#2980b9", width=3, fill="#3498db")
            self.shape_canvas.create_text(cx, cy, text="4 cạnh bằng nhau\n4 góc vuông", fill="white", font=("Arial", 8, "bold"), justify="center")
            
        elif shape_name == "Hình chữ nhật":
            self.shape_canvas.create_rectangle(cx-75, cy-45, cx+75, cy+45, outline="#2980b9", width=3, fill="#3498db")
            self.shape_canvas.create_text(cx, cy, text="4 góc vuông\nCạnh đối bằng nhau", fill="white", font=("Arial", 8, "bold"), justify="center")
            
        elif shape_name == "Hình thoi":
            points = [cx, cy-65, cx+65, cy, cx, cy+65, cx-65, cy]
            self.shape_canvas.create_polygon(points, outline="#2980b9", width=3, fill="#3498db")
            self.shape_canvas.create_text(cx, cy, text="4 cạnh bằng nhau\nĐường chéo vuông góc", fill="white", font=("Arial", 8, "bold"), justify="center")
            
        elif shape_name == "Hình bình hành":
            points = [cx-50, cy-35, cx+75, cy-35, cx+50, cy+35, cx-75, cy+35]
            self.shape_canvas.create_polygon(points, outline="#2980b9", width=3, fill="#3498db")
            self.shape_canvas.create_text(cx, cy, text="Cạnh đối song song", fill="white", font=("Arial", 8, "bold"), justify="center")
            
        elif "Hình thang" in shape_name:
            points = [cx-35, cy-35, cx+35, cy-35, cx+75, cy+35, cx-75, cy+35]
            self.shape_canvas.create_polygon(points, outline="#2980b9", width=3, fill="#3498db")
            self.shape_canvas.create_text(cx, cy, text="1 cặp cạnh đáy song song", fill="white", font=("Arial", 8, "bold"), justify="center")
            
        elif shape_name == "Hình diều":
            points = [cx, cy-75, cx+45, cy, cx, cy+55, cx-45, cy]
            self.shape_canvas.create_polygon(points, outline="#2980b9", width=3, fill="#3498db")
            self.shape_canvas.create_text(cx, cy, text="2 cặp cạnh kề bằng nhau", fill="white", font=("Arial", 8, "bold"), justify="center")
            
        else:
            points = [cx-55, cy-45, cx+65, cy-25, cx+45, cy+55, cx-65, cy+35]
            self.shape_canvas.create_polygon(points, outline="#2980b9", width=3, fill="#3498db")
            self.shape_canvas.create_text(cx, cy, text="Tứ giác tổng quát", fill="white", font=("Arial", 8, "bold"), justify="center")

    def on_closing(self):
        if hasattr(self, 'driver'):
            self.driver.close()
        self.root.destroy()

if __name__ == "__main__":
    root = tk.Tk()
    app = FullFeatureGraphApp(root)
    root.protocol("WM_DELETE_WINDOW", app.on_closing)
    root.mainloop()
