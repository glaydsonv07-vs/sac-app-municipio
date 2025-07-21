import React, { useState, useEffect } from 'react';
import { initializeApp } from 'firebase/app';
import { getAuth, signInAnonymously, signInWithCustomToken, onAuthStateChanged } from 'firebase/auth';
import { getFirestore, collection, addDoc, onSnapshot, query, orderBy, doc, deleteDoc, updateDoc } from 'firebase/firestore';

// Variáveis globais fornecidas pelo ambiente Canvas
const appId = typeof __app_id !== 'undefined' ? __app_id : 'default-app-id';
const firebaseConfig = typeof __firebase_config !== 'undefined' ? JSON.parse(__firebase_config) : {};
const initialAuthToken = typeof __initial_auth_token !== 'undefined' ? __initial_auth_token : null;

// Componente principal do aplicativo
function App() {
  const [activeTab, setActiveTab] = useState('home'); // Estado para controlar a aba ativa
  const [db, setDb] = useState(null); // Instância do Firestore
  const [auth, setAuth] = useState(null); // Instância do Auth
  const [userId, setUserId] = useState(null); // ID do usuário logado
  const [loading, setLoading] = useState(true); // Estado de carregamento da autenticação
  const [showModal, setShowModal] = useState(false); // Estado para controlar a visibilidade do modal
  const [modalMessage, setModalMessage] = useState(''); // Mensagem a ser exibida no modal

  // Efeito para inicializar o Firebase e autenticar o usuário
  useEffect(() => {
    const initFirebase = async () => {
      try {
        const app = initializeApp(firebaseConfig);
        const firestore = getFirestore(app);
        const firebaseAuth = getAuth(app);
        setDb(firestore);
        setAuth(firebaseAuth);

        // Listener para mudanças no estado de autenticação
        onAuthStateChanged(firebaseAuth, async (user) => {
          if (user) {
            setUserId(user.uid);
          } else {
            // Se não houver usuário, tenta autenticar com o token personalizado ou anonimamente
            try {
              if (initialAuthToken) {
                await signInWithCustomToken(firebaseAuth, initialAuthToken);
              } else {
                await signInAnonymously(firebaseAuth);
              }
            } catch (error) {
              console.error("Erro ao autenticar:", error);
              showInfoModal("Erro ao autenticar. Por favor, tente novamente.");
            }
          }
          setLoading(false); // Finaliza o carregamento após a tentativa de autenticação
        });
      } catch (error) {
        console.error("Erro ao inicializar Firebase:", error);
        showInfoModal("Erro ao inicializar o aplicativo. Por favor, tente novamente.");
        setLoading(false);
      }
    };

    initFirebase();
  }, []);

  // Função para exibir o modal de informações
  const showInfoModal = (message) => {
    setModalMessage(message);
    setShowModal(true);
  };

  // Componente de carregamento
  if (loading) {
    return (
      <div className="flex items-center justify-center min-h-screen bg-gray-100">
        <div className="text-xl font-semibold text-gray-700">Carregando...</div>
      </div>
    );
  }

  // Componente Modal customizado
  const Modal = ({ message, onClose }) => (
    <div className="fixed inset-0 bg-gray-600 bg-opacity-50 flex items-center justify-center z-50">
      <div className="bg-white p-6 rounded-lg shadow-xl max-w-sm w-full">
        <p className="text-lg text-gray-800 mb-4">{message}</p>
        <button
          onClick={onClose}
          className="w-full bg-blue-600 text-white py-2 px-4 rounded-md hover:bg-blue-700 transition duration-200"
        >
          Fechar
        </button>
      </div>
    </div>
  );

  return (
    <div className="min-h-screen bg-gradient-to-br from-blue-50 to-indigo-100 font-sans text-gray-800">
      {/* Modal de informações */}
      {showModal && <Modal message={modalMessage} onClose={() => setShowModal(false)} />}

      {/* Cabeçalho */}
      <header className="bg-white shadow-md py-4 px-6 flex flex-col sm:flex-row items-center justify-between sticky top-0 z-40">
        <h1 className="text-3xl font-bold text-blue-700 mb-2 sm:mb-0">Sistema de Apoio ao Cidadão (SAC)</h1>
        <nav className="flex space-x-4">
          <button
            onClick={() => setActiveTab('home')}
            className={`py-2 px-4 rounded-lg font-medium transition duration-200 ${
              activeTab === 'home' ? 'bg-blue-600 text-white shadow-md' : 'text-gray-600 hover:bg-blue-100'
            }`}
          >
            Início
          </button>
          <button
            onClick={() => setActiveTab('atendimentos')}
            className={`py-2 px-4 rounded-lg font-medium transition duration-200 ${
              activeTab === 'atendimentos' ? 'bg-blue-600 text-white shadow-md' : 'text-gray-600 hover:bg-blue-100'
            }`}
          >
            Atendimentos
          </button>
          <button
            onClick={() => setActiveTab('beneficios')}
            className={`py-2 px-4 rounded-lg font-medium transition duration-200 ${
              activeTab === 'beneficios' ? 'bg-blue-600 text-white shadow-md' : 'text-gray-600 hover:bg-blue-100'
            }`}
          >
            Benefícios
          </button>
          <button
            onClick={() => setActiveTab('relatorios')}
            className={`py-2 px-4 rounded-lg font-medium transition duration-200 ${
              activeTab === 'relatorios' ? 'bg-blue-600 text-white shadow-md' : 'text-gray-600 hover:bg-blue-100'
            }`}
          >
            Relatórios
          </button>
        </nav>
      </header>

      {/* Conteúdo principal */}
      <main className="container mx-auto p-6">
        {activeTab === 'home' && <Home />}
        {activeTab === 'atendimentos' && db && auth && userId && <Atendimento db={db} auth={auth} userId={userId} showInfoModal={showInfoModal} />}
        {activeTab === 'beneficios' && db && auth && userId && <Beneficios db={db} auth={auth} userId={userId} showInfoModal={showInfoModal} />}
        {activeTab === 'relatorios' && <Relatorios />}
      </main>

      {/* Rodapé */}
      <footer className="bg-gray-800 text-white py-4 px-6 text-center mt-8 rounded-t-lg">
        <p className="text-sm">
          Desenvolvido para o Projeto SAC - Gran Faculdade
          {userId && (
            <span className="block mt-2 text-xs text-gray-400">
              ID do Usuário: {userId}
            </span>
          )}
        </p>
      </footer>
    </div>
  );
}

// Componente Home
function Home() {
  return (
    <div className="bg-white p-8 rounded-xl shadow-lg text-center">
      <h2 className="text-4xl font-extrabold text-blue-800 mb-6">Bem-vindo ao Sistema de Apoio ao Cidadão (SAC)</h2>
      <p className="text-lg text-gray-700 leading-relaxed mb-8">
        Este aplicativo foi desenvolvido para otimizar a gestão de atendimentos e benefícios sociais,
        facilitando o trabalho da equipe social e melhorando a qualidade dos serviços públicos.
      </p>
      <div className="grid grid-cols-1 md:grid-cols-3 gap-6">
        <div className="bg-blue-50 p-6 rounded-lg shadow-md">
          <h3 className="text-xl font-semibold text-blue-700 mb-3">Gerenciar Atendimentos</h3>
          <p className="text-gray-600">Registre e acompanhe todos os atendimentos aos cidadãos de forma eficiente.</p>
        </div>
        <div className="bg-green-50 p-6 rounded-lg shadow-md">
          <h3 className="text-xl font-semibold text-green-700 mb-3">Gerenciar Benefícios</h3>
          <p className="text-gray-600">Conceda e monitore os benefícios sociais com facilidade.</p>
        </div>
        <div className="bg-purple-50 p-6 rounded-lg shadow-md">
          <h3 className="text-xl font-semibold text-purple-700 mb-3">Gerar Relatórios</h3>
          <p className="text-gray-600">Obtenha insights valiosos através de relatórios detalhados (funcionalidade futura).</p>
        </div>
      </div>
    </div>
  );
}

// Componente Atendimento
function Atendimento({ db, auth, userId, showInfoModal }) {
  const [atendimentos, setAtendimentos] = useState([]); // Lista de atendimentos
  const [nomeCidadao, setNomeCidadao] = useState(''); // Nome do cidadão
  const [descricao, setDescricao] = useState(''); // Descrição do atendimento
  const [editingId, setEditingId] = useState(null); // ID do atendimento sendo editado
  const [loading, setLoading] = useState(true); // Estado de carregamento dos dados

  // Efeito para buscar atendimentos do Firestore em tempo real
  useEffect(() => {
    if (!db || !userId) return;

    setLoading(true);
    const q = query(collection(db, `artifacts/${appId}/users/${userId}/atendimentos`));
    const unsubscribe = onSnapshot(q, (snapshot) => {
      const atendimentosData = snapshot.docs.map(doc => ({
        id: doc.id,
        ...doc.data()
      }));
      setAtendimentos(atendimentosData);
      setLoading(false);
    }, (error) => {
      console.error("Erro ao buscar atendimentos:", error);
      showInfoModal("Erro ao carregar atendimentos.");
      setLoading(false);
    });

    return () => unsubscribe(); // Limpa o listener ao desmontar o componente
  }, [db, userId, showInfoModal]);

  // Função para adicionar ou atualizar um atendimento
  const handleSubmit = async (e) => {
    e.preventDefault();
    if (!nomeCidadao || !descricao) {
      showInfoModal("Por favor, preencha todos os campos.");
      return;
    }

    try {
      if (editingId) {
        // Atualiza atendimento existente
        const atendimentoRef = doc(db, `artifacts/${appId}/users/${userId}/atendimentos`, editingId);
        await updateDoc(atendimentoRef, {
          nomeCidadao,
          descricao,
          dataAtualizacao: new Date().toISOString(),
        });
        showInfoModal("Atendimento atualizado com sucesso!");
        setEditingId(null); // Sai do modo de edição
      } else {
        // Adiciona novo atendimento
        await addDoc(collection(db, `artifacts/${appId}/users/${userId}/atendimentos`), {
          nomeCidadao,
          descricao,
          dataRegistro: new Date().toISOString(),
        });
        showInfoModal("Atendimento registrado com sucesso!");
      }
      // Limpa os campos do formulário
      setNomeCidadao('');
      setDescricao('');
    } catch (e) {
      console.error("Erro ao adicionar/atualizar atendimento: ", e);
      showInfoModal("Erro ao salvar atendimento. Por favor, tente novamente.");
    }
  };

  // Função para iniciar a edição de um atendimento
  const handleEdit = (atendimento) => {
    setNomeCidadao(atendimento.nomeCidadao);
    setDescricao(atendimento.descricao);
    setEditingId(atendimento.id);
  };

  // Função para deletar um atendimento
  const handleDelete = async (id) => {
    if (window.confirm("Tem certeza que deseja excluir este atendimento?")) { // Usando confirm para simplicidade, idealmente um modal customizado
      try {
        await deleteDoc(doc(db, `artifacts/${appId}/users/${userId}/atendimentos`, id));
        showInfoModal("Atendimento excluído com sucesso!");
      } catch (e) {
        console.error("Erro ao excluir atendimento: ", e);
        showInfoModal("Erro ao excluir atendimento. Por favor, tente novamente.");
      }
    }
  };

  return (
    <div className="bg-white p-8 rounded-xl shadow-lg">
      <h2 className="text-3xl font-bold text-blue-700 mb-6">Gerenciar Atendimentos</h2>

      {/* Formulário de Atendimento */}
      <form onSubmit={handleSubmit} className="mb-8 p-6 bg-blue-50 rounded-lg shadow-inner">
        <div className="mb-4">
          <label htmlFor="nomeCidadao" className="block text-gray-700 text-sm font-semibold mb-2">
            Nome do Cidadão:
          </label>
          <input
            type="text"
            id="nomeCidadao"
            className="shadow-sm appearance-none border rounded-md w-full py-2 px-3 text-gray-700 leading-tight focus:outline-none focus:ring-2 focus:ring-blue-400"
            value={nomeCidadao}
            onChange={(e) => setNomeCidadao(e.target.value)}
            required
          />
        </div>
        <div className="mb-4">
          <label htmlFor="descricao" className="block text-gray-700 text-sm font-semibold mb-2">
            Descrição do Atendimento:
          </label>
          <textarea
            id="descricao"
            className="shadow-sm appearance-none border rounded-md w-full py-2 px-3 text-gray-700 leading-tight focus:outline-none focus:ring-2 focus:ring-blue-400 h-24"
            value={descricao}
            onChange={(e) => setDescricao(e.target.value)}
            required
          ></textarea>
        </div>
        <button
          type="submit"
          className="bg-blue-600 hover:bg-blue-700 text-white font-bold py-2 px-6 rounded-lg shadow-md transition duration-200"
        >
          {editingId ? 'Atualizar Atendimento' : 'Registrar Atendimento'}
        </button>
        {editingId && (
          <button
            type="button"
            onClick={() => { setEditingId(null); setNomeCidadao(''); setDescricao(''); }}
            className="ml-4 bg-gray-400 hover:bg-gray-500 text-white font-bold py-2 px-6 rounded-lg shadow-md transition duration-200"
          >
            Cancelar Edição
          </button>
        )}
      </form>

      {/* Lista de Atendimentos */}
      <h3 className="text-2xl font-bold text-blue-700 mb-4">Atendimentos Registrados</h3>
      {loading ? (
        <p className="text-gray-600">Carregando atendimentos...</p>
      ) : atendimentos.length === 0 ? (
        <p className="text-gray-600">Nenhum atendimento registrado ainda.</p>
      ) : (
        <div className="overflow-x-auto">
          <table className="min-w-full bg-white rounded-lg shadow-md">
            <thead>
              <tr className="bg-blue-600 text-white">
                <th className="py-3 px-4 text-left rounded-tl-lg">Nome do Cidadão</th>
                <th className="py-3 px-4 text-left">Descrição</th>
                <th className="py-3 px-4 text-left">Data de Registro</th>
                <th className="py-3 px-4 text-left rounded-tr-lg">Ações</th>
              </tr>
            </thead>
            <tbody>
              {atendimentos.map((atendimento) => (
                <tr key={atendimento.id} className="border-b border-gray-200 hover:bg-gray-50">
                  <td className="py-3 px-4">{atendimento.nomeCidadao}</td>
                  <td className="py-3 px-4">{atendimento.descricao}</td>
                  <td className="py-3 px-4">
                    {new Date(atendimento.dataRegistro).toLocaleDateString()}
                  </td>
                  <td className="py-3 px-4 flex space-x-2">
                    <button
                      onClick={() => handleEdit(atendimento)}
                      className="bg-yellow-500 hover:bg-yellow-600 text-white py-1 px-3 rounded-md text-sm transition duration-200"
                    >
                      Editar
                    </button>
                    <button
                      onClick={() => handleDelete(atendimento.id)}
                      className="bg-red-500 hover:bg-red-600 text-white py-1 px-3 rounded-md text-sm transition duration-200"
                    >
                      Excluir
                    </button>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}
    </div>
  );
}

// Componente Beneficios
function Beneficios({ db, auth, userId, showInfoModal }) {
  const [beneficios, setBeneficios] = useState([]); // Lista de benefícios
  const [nomeCidadao, setNomeCidadao] = useState(''); // Nome do cidadão
  const [tipoBeneficio, setTipoBeneficio] = useState(''); // Tipo de benefício
  const [statusBeneficio, setStatusBeneficio] = useState('Pendente'); // Status do benefício
  const [editingId, setEditingId] = useState(null); // ID do benefício sendo editado
  const [loading, setLoading] = useState(true); // Estado de carregamento dos dados

  // Efeito para buscar benefícios do Firestore em tempo real
  useEffect(() => {
    if (!db || !userId) return;

    setLoading(true);
    const q = query(collection(db, `artifacts/${appId}/users/${userId}/beneficios`));
    const unsubscribe = onSnapshot(q, (snapshot) => {
      const beneficiosData = snapshot.docs.map(doc => ({
        id: doc.id,
        ...doc.data()
      }));
      setBeneficios(beneficiosData);
      setLoading(false);
    }, (error) => {
      console.error("Erro ao buscar benefícios:", error);
      showInfoModal("Erro ao carregar benefícios.");
      setLoading(false);
    });

    return () => unsubscribe(); // Limpa o listener ao desmontar o componente
  }, [db, userId, showInfoModal]);

  // Função para adicionar ou atualizar um benefício
  const handleSubmit = async (e) => {
    e.preventDefault();
    if (!nomeCidadao || !tipoBeneficio) {
      showInfoModal("Por favor, preencha todos os campos obrigatórios.");
      return;
    }

    try {
      if (editingId) {
        // Atualiza benefício existente
        const beneficioRef = doc(db, `artifacts/${appId}/users/${userId}/beneficios`, editingId);
        await updateDoc(beneficioRef, {
          nomeCidadao,
          tipoBeneficio,
          statusBeneficio,
          dataAtualizacao: new Date().toISOString(),
        });
        showInfoModal("Benefício atualizado com sucesso!");
        setEditingId(null); // Sai do modo de edição
      } else {
        // Adiciona novo benefício
        await addDoc(collection(db, `artifacts/${appId}/users/${userId}/beneficios`), {
          nomeCidadao,
          tipoBeneficio,
          statusBeneficio,
          dataConcessao: new Date().toISOString(),
        });
        showInfoModal("Benefício registrado com sucesso!");
      }
      // Limpa os campos do formulário
      setNomeCidadao('');
      setTipoBeneficio('');
      setStatusBeneficio('Pendente');
    } catch (e) {
      console.error("Erro ao adicionar/atualizar benefício: ", e);
      showInfoModal("Erro ao salvar benefício. Por favor, tente novamente.");
    }
  };

  // Função para iniciar a edição de um benefício
  const handleEdit = (beneficio) => {
    setNomeCidadao(beneficio.nomeCidadao);
    setTipoBeneficio(beneficio.tipoBeneficio);
    setStatusBeneficio(beneficio.statusBeneficio);
    setEditingId(beneficio.id);
  };

  // Função para deletar um benefício
  const handleDelete = async (id) => {
    if (window.confirm("Tem certeza que deseja excluir este benefício?")) { // Usando confirm para simplicidade, idealmente um modal customizado
      try {
        await deleteDoc(doc(db, `artifacts/${appId}/users/${userId}/beneficios`, id));
        showInfoModal("Benefício excluído com sucesso!");
      } catch (e) {
        console.error("Erro ao excluir benefício: ", e);
        showInfoModal("Erro ao excluir benefício. Por favor, tente novamente.");
      }
    }
  };

  return (
    <div className="bg-white p-8 rounded-xl shadow-lg">
      <h2 className="text-3xl font-bold text-green-700 mb-6">Gerenciar Benefícios</h2>

      {/* Formulário de Benefício */}
      <form onSubmit={handleSubmit} className="mb-8 p-6 bg-green-50 rounded-lg shadow-inner">
        <div className="mb-4">
          <label htmlFor="nomeCidadaoBeneficio" className="block text-gray-700 text-sm font-semibold mb-2">
            Nome do Cidadão:
          </label>
          <input
            type="text"
            id="nomeCidadaoBeneficio"
            className="shadow-sm appearance-none border rounded-md w-full py-2 px-3 text-gray-700 leading-tight focus:outline-none focus:ring-2 focus:ring-green-400"
            value={nomeCidadao}
            onChange={(e) => setNomeCidadao(e.target.value)}
            required
          />
        </div>
        <div className="mb-4">
          <label htmlFor="tipoBeneficio" className="block text-gray-700 text-sm font-semibold mb-2">
            Tipo de Benefício:
          </label>
          <input
            type="text"
            id="tipoBeneficio"
            className="shadow-sm appearance-none border rounded-md w-full py-2 px-3 text-gray-700 leading-tight focus:outline-none focus:ring-2 focus:ring-green-400"
            value={tipoBeneficio}
            onChange={(e) => setTipoBeneficio(e.target.value)}
            required
          />
        </div>
        <div className="mb-4">
          <label htmlFor="statusBeneficio" className="block text-gray-700 text-sm font-semibold mb-2">
            Status:
          </label>
          <select
            id="statusBeneficio"
            className="shadow-sm appearance-none border rounded-md w-full py-2 px-3 text-gray-700 leading-tight focus:outline-none focus:ring-2 focus:ring-green-400"
            value={statusBeneficio}
            onChange={(e) => setStatusBeneficio(e.target.value)}
          >
            <option value="Pendente">Pendente</option>
            <option value="Aprovado">Aprovado</option>
            <option value="Negado">Negado</option>
            <option value="Concluído">Concluído</option>
          </select>
        </div>
        <button
          type="submit"
          className="bg-green-600 hover:bg-green-700 text-white font-bold py-2 px-6 rounded-lg shadow-md transition duration-200"
        >
          {editingId ? 'Atualizar Benefício' : 'Registrar Benefício'}
        </button>
        {editingId && (
          <button
            type="button"
            onClick={() => { setEditingId(null); setNomeCidadao(''); setTipoBeneficio(''); setStatusBeneficio('Pendente'); }}
            className="ml-4 bg-gray-400 hover:bg-gray-500 text-white font-bold py-2 px-6 rounded-lg shadow-md transition duration-200"
          >
            Cancelar Edição
          </button>
        )}
      </form>

      {/* Lista de Benefícios */}
      <h3 className="text-2xl font-bold text-green-700 mb-4">Benefícios Registrados</h3>
      {loading ? (
        <p className="text-gray-600">Carregando benefícios...</p>
      ) : beneficios.length === 0 ? (
        <p className="text-gray-600">Nenhum benefício registrado ainda.</p>
      ) : (
        <div className="overflow-x-auto">
          <table className="min-w-full bg-white rounded-lg shadow-md">
            <thead>
              <tr className="bg-green-600 text-white">
                <th className="py-3 px-4 text-left rounded-tl-lg">Nome do Cidadão</th>
                <th className="py-3 px-4 text-left">Tipo de Benefício</th>
                <th className="py-3 px-4 text-left">Status</th>
                <th className="py-3 px-4 text-left">Data de Concessão</th>
                <th className="py-3 px-4 text-left rounded-tr-lg">Ações</th>
              </tr>
            </thead>
            <tbody>
              {beneficios.map((beneficio) => (
                <tr key={beneficio.id} className="border-b border-gray-200 hover:bg-gray-50">
                  <td className="py-3 px-4">{beneficio.nomeCidadao}</td>
                  <td className="py-3 px-4">{beneficio.tipoBeneficio}</td>
                  <td className="py-3 px-4">
                    <span className={`px-3 py-1 rounded-full text-xs font-semibold ${
                      beneficio.statusBeneficio === 'Aprovado' ? 'bg-green-200 text-green-800' :
                      beneficio.statusBeneficio === 'Pendente' ? 'bg-yellow-200 text-yellow-800' :
                      beneficio.statusBeneficio === 'Negado' ? 'bg-red-200 text-red-800' :
                      'bg-gray-200 text-gray-800'
                    }`}>
                      {beneficio.statusBeneficio}
                    </span>
                  </td>
                  <td className="py-3 px-4">
                    {new Date(beneficio.dataConcessao).toLocaleDateString()}
                  </td>
                  <td className="py-3 px-4 flex space-x-2">
                    <button
                      onClick={() => handleEdit(beneficio)}
                      className="bg-yellow-500 hover:bg-yellow-600 text-white py-1 px-3 rounded-md text-sm transition duration-200"
                    >
                      Editar
                    </button>
                    <button
                      onClick={() => handleDelete(beneficio.id)}
                      className="bg-red-500 hover:bg-red-600 text-white py-1 px-3 rounded-md text-sm transition duration-200"
                    >
                      Excluir
                    </button>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}
    </div>
  );
}

// Componente Relatórios
function Relatorios() {
  return (
    <div className="bg-white p-8 rounded-xl shadow-lg text-center">
      <h2 className="text-3xl font-bold text-purple-700 mb-6">Gerar Relatórios</h2>
      <p className="text-lg text-gray-700 leading-relaxed mb-4">
        Esta seção está em desenvolvimento. Futuramente, você poderá gerar relatórios detalhados
        sobre atendimentos e benefícios para obter insights valiosos.
      </p>
      <p className="text-md text-gray-600">
        Aguarde por novas atualizações!
      </p>
    </div>
  );
}

export default App;