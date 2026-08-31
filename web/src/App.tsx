import { IssueList } from "./components/IssueList";
import "./App.css";

function App() {
  return (
    <div className="app">
      <header className="app-header">
        <h1>🏗️ 巡检问题管理</h1>
        <p>现场巡检问题标注系统</p>
      </header>
      <main className="app-main">
        <IssueList />
      </main>
    </div>
  );
}

export default App;
