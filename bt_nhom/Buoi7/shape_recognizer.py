import tkinter as tk
from neo4j import GraphDatabase
import networkx as nx

# --- CẤU HÌNH KẾT NỐI NEO4J ---
URI = "neo4j://127.0.0.1:7687"
AUTH = ("neo4j", "123456789") # Sửa lại mật khẩu của bạn

class InteractiveGraphApp:
    def __init__(self, root):
        self.root = root
        self.root.title("Graph Visualization & Hình học trực quan")
        self.root.geometry("1000x850")

        self.nodes_data = {}  
        self.edges_data = []  
        self.drag_data = {"node": None, "start_x": 0, "start_y": 0}
        self.node_radius = 42 

        self.setup_ui()
        self.load_data_from_neo4j()

    def setup_ui(self):
        # 1. Khu vực biểu đồ Graph (Nửa trên)
        self.canvas = tk.Canvas(self.root, bg="#f0f0f0", height=450)
        self.canvas.pack(fill=tk.BOTH, expand=True)

        # 2. Bảng thông tin và vẽ hình (Nửa dưới)
        self.frame_bottom = tk.Frame(self.root, height=320, bg="white", padx=15, pady=10)
        self.frame_bottom.pack(fill=tk.X, side=tk.BOTTOM)
        self.frame_bottom.pack_propagate(False)

        # Tiêu đề node đang chọn
        self.lbl_selected_node = tk.Label(self.frame_bottom, text="HÃY CLICK VÀO MỘT HÌNH TRÊN BIỂU ĐỒ", font=("Arial", 12, "bold"), fg="#c0392b", bg="white")
        self.lbl_selected_node.pack(anchor="w", pady=(0, 5))

        # Chia khung dưới thành 2 cột (Trái: Text thông tin, Phải: Canvas vẽ hình)
        self.frame_left_info = tk.Frame(self.frame_bottom, bg="white", width=550)
        self.frame_left_info.pack(side=tk.LEFT, fill=tk.BOTH, expand=True)

        self.frame_right_draw = tk.Frame(self.frame_bottom, bg="#ecf0f1", width=380, bd=1, relief="solid")
        self.frame_right_draw.pack(side=tk.RIGHT, fill=tk.BOTH, padx=10)
        self.frame_right_draw.pack_propagate(False)

        # Nội dung cột trái (Thông tin từ Neo4j)
        tk.Label(self.frame_left_info, text="Tính chất:", font=("Arial", 10, "bold"), bg="white").pack(anchor="w")
        self.lbl_tinh_chat_val = tk.Label(self.frame_left_info, text="", font=("Arial", 9), justify="left", wraplength=520, bg="white")
        self.lbl_tinh_chat_val.pack(anchor="w", pady=(0, 5))

        tk.Label(self.frame_left_info, text="Công thức Toán học:", font=("Arial", 10, "bold"), bg="white").pack(anchor="w")
        self.lbl_math_val = tk.Label(self.frame_left_info, text="", font=("Arial", 9), justify="left", bg="white")
        self.lbl_math_val.pack(anchor="w")

        # Nội dung cột phải (Canvas vẽ hình minh họa)
        tk.Label(self.frame_right_draw, text="Mô phỏng hình học", font=("Arial", 9, "bold"), bg="#ecf0f1", fg="#2c3e50").pack(pady=5)
        self.shape_canvas = tk.Canvas(self.frame_right_draw, bg="white", width=350, height=240)
        self.shape_canvas.pack(pady=5)

        # Gắn sự kiện chuột cho Canvas Graph chính
        self.canvas.bind("<ButtonPress-1>", self.on_press)
        self.canvas.bind("<B1-Motion>", self.on_drag)
        self.canvas.bind("<ButtonRelease-1>", self.on_release)

    def load_data_from_neo4j(self):
        self.driver = GraphDatabase.driver(URI, auth=AUTH)
        try:
            with self.driver.session() as session:
                query = "MATCH (n:Shape {num_sides: 4})-[r:LA_DANG_DAC_BIET_CUA]->(m:Shape) RETURN n.name AS source, m.name AS target"
                edges = [(r["source"], r["target"]) for r in session.run(query)]
        except Exception as e:
            self.lbl_selected_node.config(text=f"Lỗi kết nối CSDL: {e}")
            return

        G = nx.DiGraph()
        G.add_edges_from(edges)
        pos = nx.spring_layout(G, seed=42)

        self.root.update()
        canvas_width = self.canvas.winfo_width()
        canvas_height = 400

        for src, tgt in edges:
            line_id = self.canvas.create_line(0, 0, 0, 0, arrow=tk.LAST, width=2, fill="gray", arrowshape=(12, 15, 4))
            self.edges_data.append({'line': line_id, 'src': src, 'tgt': tgt})

        for node, (x, y) in pos.items():
            px = (x + 1) * (canvas_width / 2 * 0.8) + 50
            py = (y + 1) * (canvas_height / 2 * 0.8) + 30

            oval_id = self.canvas.create_oval(px - self.node_radius, py - self.node_radius, 
                                              px + self.node_radius, py + self.node_radius, 
                                              fill="#3498db", outline="black", width=2)
            
            display_name = node.replace(" ", "\n") 
            text_id = self.canvas.create_text(px, py, text=display_name, font=("Arial", 8, "bold"), fill="white", justify="center")

            self.nodes_data[node] = {'oval': oval_id, 'text': text_id, 'x': px, 'y': py}

        self.update_edges_positions()

    def update_edges_positions(self):
        for edge in self.edges_data:
            src_node = self.nodes_data[edge['src']]
            tgt_node = self.nodes_data[edge['tgt']]
            self.canvas.coords(edge['line'], src_node['x'], src_node['y'], tgt_node['x'], tgt_node['y'])

    def on_press(self, event):
        clicked_node = None
        for node_name, data in self.nodes_data.items():
            dist = ((event.x - data['x'])**2 + (event.y - data['y'])**2)**0.5
            if dist <= self.node_radius:
                clicked_node = node_name
                break

        if clicked_node:
            self.drag_data["node"] = clicked_node
            self.drag_data["start_x"] = event.x
            self.drag_data["start_y"] = event.y
            
            for n_data in self.nodes_data.values():
                self.canvas.itemconfig(n_data['oval'], fill="#3498db")
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

    def fetch_node_details(self, shape_name):
        self.lbl_selected_node.config(text=f"ĐANG CHỌN: {shape_name.upper()}")
        try:
            with self.driver.session() as session:
                query = "MATCH (s:Shape {name: $name}) RETURN s.tinh_chat AS tc, s.chu_vi AS cv, s.dien_tich AS dt"
                record = session.run(query, name=shape_name).single()
                
                if record:
                    self.lbl_tinh_chat_val.config(text=record["tc"] if record["tc"] else "Đang cập nhật")
                    cv = record["cv"] if record["cv"] else "N/A"
                    dt = record["dt"] if record["dt"] else "N/A"
                    self.lbl_math_val.config(text=f"• Chu vi: {cv}\n• Diện tích: {dt}")
        except Exception as e:
            self.lbl_tinh_chat_val.config(text=f"Lỗi truy vấn: {e}")

    def draw_shape_illustration(self, shape_name):
        """Hàm vẽ mô phỏng hình học dựa trên tên hình được chọn"""
        self.shape_canvas.delete("all") # Xóa hình vẽ cũ
        
        # Tọa độ trung tâm khung vẽ phụ (350x240) -> Tâm là (175, 120)
        cx, cy = 175, 120
        
        if shape_name == "Hình vuông":
            self.shape_canvas.create_rectangle(cx-60, cy-60, cx+60, cy+60, outline="#2980b9", width=3, fill="#3498db")
            self.shape_canvas.create_text(cx, cy, text="a = a", fill="white", font=("Arial", 10, "bold"))
            
        elif shape_name == "Hình chữ nhật":
            self.shape_canvas.create_rectangle(cx-80, cy-50, cx+80, cy+50, outline="#2980b9", width=3, fill="#3498db")
            self.shape_canvas.create_text(cx, cy, text="Chiều dài & Rộng", fill="white", font=("Arial", 9, "bold"))
            
        elif shape_name == "Hình thoi":
            # Vẽ hình thoi bằng đa giác (Polygon)
            points = [cx, cy-70, cx+70, cy, cx, cy+70, cx-70, cy]
            self.shape_canvas.create_polygon(points, outline="#2980b9", width=3, fill="#3498db")
            self.shape_canvas.create_text(cx, cy, text="Đường chéo d1, d2", fill="white", font=("Arial", 8, "bold"))
            
        elif shape_name == "Hình bình hành":
            points = [cx-50, cy-40, cx+80, cy-40, cx+50, cy+40, cx-80, cy+40]
            self.shape_canvas.create_polygon(points, outline="#2980b9", width=3, fill="#3498db")
            self.shape_canvas.create_text(cx, cy, text="Đáy a, Cao h", fill="white", font=("Arial", 9, "bold"))
            
        elif "Hình thang" in shape_name:
            # Vẽ hình thang cân / thường / vuông
            points = [cx-40, cy-40, cx+40, cy-40, cx+80, cy+40, cx-80, cy+40]
            self.shape_canvas.create_polygon(points, outline="#2980b9", width=3, fill="#3498db")
            self.shape_canvas.create_text(cx, cy, text="Đáy lớn a, Đáy nhỏ b", fill="white", font=("Arial", 8, "bold"))
            
        elif shape_name == "Hình diều":
            points = [cx, cy-80, cx+50, cy, cx, cy+60, cx-50, cy]
            self.shape_canvas.create_polygon(points, outline="#2980b9", width=3, fill="#3498db")
            self.shape_canvas.create_text(cx, cy, text="Hình diều", fill="white", font=("Arial", 9, "bold"))
            
        else: # Tứ giác thường hoặc mặc định
            points = [cx-60, cy-50, cx+70, cy-30, cx+50, cy+60, cx-70, cy+40]
            self.shape_canvas.create_polygon(points, outline="#2980b9", width=3, fill="#3498db")
            self.shape_canvas.create_text(cx, cy, text="Tứ giác tổng quát", fill="white", font=("Arial", 9, "bold"))

    def on_closing(self):
        if hasattr(self, 'driver'):
            self.driver.close()
        self.root.destroy()

if __name__ == "__main__":
    root = tk.Tk()
    app = InteractiveGraphApp(root)
    root.protocol("WM_DELETE_WINDOW", app.on_closing)
    root.mainloop()